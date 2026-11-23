using Microsoft.AspNetCore.Authorization;
using Money.Management.DomainServices;
using Money.Management.Entities;
using Money.Management.Enums;
using Money.Management.Permissions;
using Money.Management.Services.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Money.Management.Services
{
    /// <summary>
    /// The current lender's business balance, growth and monthly forecast.
    /// Cash on hand = opening balance + capital added − capital withdrawn − principal lent + repayments received.
    /// </summary>
    [Authorize(MoneyManagementPermissions.Dashboard.Default)]
    public class BusinessAppService : ApplicationService, IBusinessAppService
    {
        // Loans that never paid out any money.
        private static readonly CreditStatus[] NotDisbursed = { CreditStatus.Pending, CreditStatus.Rejected, CreditStatus.Cancelled };

        private readonly IRepository<CapitalTransaction, Guid> _capitalRepository;
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly IRepository<Payment, Guid> _paymentRepository;
        private readonly LenderManager _lenderManager;

        public BusinessAppService(
            IRepository<CapitalTransaction, Guid> capitalRepository,
            IRepository<Credit, Guid> creditRepository,
            IRepository<Payment, Guid> paymentRepository,
            LenderManager lenderManager)
        {
            _capitalRepository = capitalRepository;
            _creditRepository = creditRepository;
            _paymentRepository = paymentRepository;
            _lenderManager = lenderManager;
        }

        public async Task<List<CapitalTransactionDto>> GetCapitalTransactionsAsync()
        {
            var lenderId = await CurrentLenderIdAsync();
            var items = await _capitalRepository.GetListAsync(x => x.LenderId == lenderId);
            return items
                .OrderByDescending(x => x.TransactionDate)
                .ThenByDescending(x => x.CreationTime)
                .Select(ToDto)
                .ToList();
        }

        public async Task<CapitalTransactionDto> CreateCapitalTransactionAsync(CreateCapitalTransactionDto input)
        {
            if (input.Amount <= 0)
                throw new UserFriendlyException("Enter an amount greater than 0.");

            var lenderId = await CurrentLenderIdAsync();

            if (input.Type == CapitalTransactionType.OpeningBalance &&
                await _capitalRepository.AnyAsync(x => x.LenderId == lenderId && x.Type == CapitalTransactionType.OpeningBalance))
                throw new UserFriendlyException("Your opening balance is already set. Delete it first, or record money added or taken out.");

            var entity = new CapitalTransaction(
                GuidGenerator.Create(),
                lenderId,
                input.Type,
                input.Amount,
                input.TransactionDate == default ? Clock.Now : input.TransactionDate,
                string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim());

            await _capitalRepository.InsertAsync(entity, autoSave: true);
            return ToDto(entity);
        }

        public async Task DeleteCapitalTransactionAsync(Guid id)
        {
            var lenderId = await CurrentLenderIdAsync();
            var entity = await _capitalRepository.GetAsync(id);
            if (entity.LenderId != lenderId)
                throw new UserFriendlyException("You can only remove your own business transactions.");

            await _capitalRepository.DeleteAsync(entity);
        }

        public async Task<BusinessSummaryDto> GetSummaryAsync(int months = 6)
        {
            months = Math.Clamp(months, 1, 24);
            var lenderId = await CurrentLenderIdAsync();
            var today = Clock.Now.Date;

            var capital = await _capitalRepository.GetListAsync(x => x.LenderId == lenderId);
            var credits = (await _creditRepository.GetListAsync(x => x.LenderId == lenderId))
                .Where(c => !NotDisbursed.Contains(c.Status))
                .ToList();
            var payments = await _paymentRepository.GetListAsync(x => x.LenderId == lenderId);

            var opening = capital.FirstOrDefault(x => x.Type == CapitalTransactionType.OpeningBalance);
            var added = capital.Where(x => x.Type == CapitalTransactionType.Deposit).Sum(x => x.Amount);
            var withdrawn = capital.Where(x => x.Type == CapitalTransactionType.Withdrawal).Sum(x => x.Amount);
            var netCapital = capital.Sum(x => x.SignedAmount);

            var totalLent = credits.Sum(c => c.PrincipalAmount);
            var totalRepaid = payments.Sum(p => p.Amount);
            var outstanding = credits.Sum(Receivable);

            var cash = netCapital - totalLent + totalRepaid;
            var value = cash + outstanding;

            var dto = new BusinessSummaryDto
            {
                HasOpeningBalance = opening != null,
                OpeningBalance = opening?.Amount ?? 0,
                OpeningDate = opening?.TransactionDate,
                CapitalAdded = added,
                CapitalWithdrawn = withdrawn,
                NetCapital = netCapital,
                TotalLent = totalLent,
                TotalRepaid = totalRepaid,
                CashOnHand = cash,
                OutstandingOwed = outstanding,
                BusinessValue = value,
                GrowthAmount = value - netCapital,
                GrowthPercent = netCapital > 0 ? Math.Round((value - netCapital) / netCapital * 100, 1) : 0,
            };

            // Month-end positions, oldest first; the current month is "as of today".
            var currentMonth = new DateTime(today.Year, today.Month, 1);
            for (var i = months - 1; i >= 0; i--)
            {
                var monthStart = currentMonth.AddMonths(-i);
                var monthEnd = i == 0 ? today.AddDays(1) : monthStart.AddMonths(1); // exclusive

                var capitalToDate = capital.Where(x => x.TransactionDate < monthEnd).Sum(x => x.SignedAmount);
                var issuedToDate = credits.Where(c => c.IssuedDate < monthEnd).ToList();
                var paymentsToDate = payments.Where(p => p.PaymentDate < monthEnd).ToList();

                var monthCash = capitalToDate - issuedToDate.Sum(c => c.PrincipalAmount) + paymentsToDate.Sum(p => p.Amount);
                var monthOwed = issuedToDate.Sum(c =>
                    c.Status == CreditStatus.WriteOff
                        ? 0
                        : Math.Max(0, c.TotalAmount - paymentsToDate.Where(p => p.CreditId == c.Id).Sum(p => p.Amount)));

                dto.Months.Add(new BusinessMonthDto
                {
                    Year = monthStart.Year,
                    Month = monthStart.Month,
                    CashOnHand = monthCash,
                    OutstandingOwed = monthOwed,
                    BusinessValue = monthCash + monthOwed,
                    LentInMonth = credits.Where(c => c.IssuedDate >= monthStart && c.IssuedDate < monthEnd).Sum(c => c.PrincipalAmount),
                    RepaidInMonth = payments.Where(p => p.PaymentDate >= monthStart && p.PaymentDate < monthEnd).Sum(p => p.Amount),
                });
            }

            dto.Forecast = BuildForecast(credits, payments, currentMonth);
            return dto;
        }

        private static BusinessForecastDto BuildForecast(List<Credit> credits, List<Payment> payments, DateTime monthStart)
        {
            var monthEnd = monthStart.AddMonths(1);
            var creditById = credits.ToDictionary(c => c.Id);

            // Share of each rand repaid that is profit (flat interest: interest / total).
            decimal ProfitShare(Credit c) => c.TotalAmount > 0 ? c.InterestAmount / c.TotalAmount : 0;

            var received = payments.Where(p => p.PaymentDate >= monthStart && p.PaymentDate < monthEnd).ToList();
            var receivedAmount = received.Sum(p => p.Amount);
            var earned = received.Sum(p => creditById.TryGetValue(p.CreditId, out var c) ? p.Amount * ProfitShare(c) : 0);

            var open = credits.Where(c => Receivable(c) > 0).ToList();
            var dueThisMonth = open.Where(c => c.DueDate >= monthStart && c.DueDate < monthEnd).ToList();
            var overdue = open.Where(c => c.DueDate < monthStart).ToList();

            var remaining = dueThisMonth.Sum(Receivable);
            var remainingProfit = dueThisMonth.Sum(c => Receivable(c) * ProfitShare(c));

            return new BusinessForecastDto
            {
                Year = monthStart.Year,
                Month = monthStart.Month,
                ReceivedThisMonth = receivedAmount,
                RemainingToCollect = remaining,
                ExpectedCollections = receivedAmount + remaining,
                EarnedThisMonth = Math.Round(earned, 2),
                ExpectedProfit = Math.Round(earned + remainingProfit, 2),
                LoansDueCount = dueThisMonth.Count,
                OverdueOutstanding = overdue.Sum(Receivable),
                OverdueLoansCount = overdue.Count,
            };
        }

        /// <summary>What the borrower still owes on a disbursed loan (written-off loans count as nothing).</summary>
        private static decimal Receivable(Credit c) =>
            c.Status is CreditStatus.WriteOff or CreditStatus.PaidOff ? 0 : Math.Max(0, c.OutstandingAmount);

        private async Task<Guid> CurrentLenderIdAsync()
        {
            try
            {
                return (await _lenderManager.GetCurrentLenderAsync()).Id;
            }
            catch (Exception ex) when (ex is not UserFriendlyException)
            {
                throw new UserFriendlyException("Only lenders have a business balance. Apply to become a lender first.");
            }
        }

        private static CapitalTransactionDto ToDto(CapitalTransaction x) => new()
        {
            Id = x.Id,
            Type = x.Type,
            Amount = x.Amount,
            TransactionDate = x.TransactionDate,
            Notes = x.Notes,
        };
    }
}
