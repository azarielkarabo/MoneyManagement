using Money.Management.CustomExceptions;
using Money.Management.DomainServices;
using Money.Management.Entities;
using Money.Management.Enums;
using Money.Management.Permissions;
using Money.Management.Services.credits;
using Money.Management.Services.credits.Dtos;
using Money.Management.Settings;
using Volo.Abp.Settings;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Money.Management.Services
{
    public class CreditAppService : ApplicationService, ICreditAppService
    {
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IRepository<Payment, Guid> _paymentRepository;
        private readonly IRepository<CreditTopUp, Guid> _topUpRepository;
        private readonly LenderManager _lenderManager;

        public CreditAppService(
            IRepository<Credit, Guid> creditRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            IRepository<Payment, Guid> paymentRepository,
            IRepository<CreditTopUp, Guid> topUpRepository,
            CreditManager MoneyManager,
            BorrowerManager borrowerManager,
            LenderManager lenderManager)
        {
            _creditRepository = creditRepository;
            _borrowerRepository = borrowerRepository;
            _paymentRepository = paymentRepository;
            _topUpRepository = topUpRepository;
            _lenderManager = lenderManager;
        }

        private async Task<Guid> GetCurrentLenderIdAsync()
        {
            var lender = await _lenderManager.GetCurrentLenderAsync();
            return lender.Id;
        }


        // Admins see everything (null = no lender filter); lenders only see their own data.
        private async Task<Guid?> GetLenderScopeAsync()
        {
            if (CurrentUser.IsInRole("admin"))
            {
                return null;
            }
            return (await _lenderManager.GetCurrentLenderAsync()).Id;
        }

        [Authorize(MoneyManagementPermissions.Credits.Default)]
        public async Task<CreditDto> GetAsync(Guid id)
        {
            var credit = await _creditRepository.GetAsync(id);
            var dto = ObjectMapper.Map<Credit, CreditDto>(credit);

            var borrower = await _borrowerRepository.GetAsync(credit.BorrowerId);
            dto.BorrowerName = borrower.FullName;

            // Calculate payment progress
            dto.PaymentProgress = dto.TotalAmount > 0
                ? (dto.AmountPaid / dto.TotalAmount) * 100
                : 0;

            return dto;
        }

        [Authorize(MoneyManagementPermissions.Credits.Default)]
        public async Task<PagedResultDto<CreditDto>> GetListAsync(GetCreditsInput input)
        {
            var queryable = await _creditRepository.GetQueryableAsync();

            if (input.BorrowerId.HasValue)
            {
                queryable = queryable.Where(l => l.BorrowerId == input.BorrowerId.Value);
            }

            if (input.Status.HasValue)
            {
                queryable = queryable.Where(l => l.Status == input.Status.Value);
            }

            if (input.FromDate.HasValue)
            {
                queryable = queryable.Where(l => l.IssuedDate >= input.FromDate.Value);
            }

            if (input.ToDate.HasValue)
            {
                queryable = queryable.Where(l => l.IssuedDate <= input.ToDate.Value);
            }

            if (input.MinAmount.HasValue)
            {
                queryable = queryable.Where(l => l.TotalAmount >= input.MinAmount.Value);
            }

            if (input.MaxAmount.HasValue)
            {
                queryable = queryable.Where(l => l.TotalAmount <= input.MaxAmount.Value);
            }

            if (input.IsOverdue.HasValue && input.IsOverdue.Value)
            {
                queryable = queryable.Where(l => l.Status == CreditStatus.Overdue);
            }

            var lenderId = await GetLenderScopeAsync();

            queryable = queryable.Where(i => (lenderId == null || i.LenderId == lenderId));

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable.OrderByDescending(l => l.IssuedDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var credits = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Credit>, List<CreditDto>>(credits);

            var borrowerIds = credits.Select(c => c.BorrowerId).Distinct().ToList();
            var borrowers = await _borrowerRepository.GetListAsync(b => borrowerIds.Contains(b.Id));
            var borrowerNamesById = borrowers.ToDictionary(b => b.Id, b => b.FullName);

            foreach (var dto in dtos)
            {
                dto.BorrowerName = borrowerNamesById.GetValueOrDefault(dto.BorrowerId);
            }

            return new PagedResultDto<CreditDto>(totalCount, dtos);
        }

        [Authorize(MoneyManagementPermissions.Credits.Create)]
        public async Task<CreditDto> CreateAsync(CreateCreditDto input)
        {
            EnsureNotAdmin();
            var lenderId = await GetCurrentLenderIdAsync(); // only a lender can create a loan
            var credit = new Credit(
                GuidGenerator.Create(),
                input.BorrowerId,
                lenderId,
                input.PrincipalAmount,
                input.InterestRate,
                input.TermMonths,
                input.DueDate,
                input.Purpose
            );
            credit.Notes = input.Description;
            credit.RepaymentType = input.RepaymentType;

            await _creditRepository.InsertAsync(credit);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Update borrower financials
            await UpdateBorrowerFinancialsAsync(input.BorrowerId);

            return await GetAsync(credit.Id);
        }

        [Authorize(MoneyManagementPermissions.Credits.Edit)]
        public async Task<CreditDto> UpdateAsync(Guid id, UpdateCreditDto input)
        {
            EnsureNotAdmin();
            var credit = await _creditRepository.GetAsync(id);

            // Only allow updates if credit is not paid off
            if (credit.Status == CreditStatus.PaidOff)
            {
                throw new CannotUpdatePaidOffMoneyException(id);
            }

            // The corrected total cannot be less than what the borrower has already paid.
            var newTotal = input.PrincipalAmount + input.PrincipalAmount * (input.InterestRate / 100);
            var amountsEdited = credit.PrincipalAmount != input.PrincipalAmount || credit.InterestRate != input.InterestRate;
            if (amountsEdited && newTotal < credit.AmountPaid)
            {
                throw new UserFriendlyException(
                    $"The new total ({newTotal:N2}) cannot be less than the amount already paid ({credit.AmountPaid:N2}).");
            }

            // Only recalculate interest when the principal or rate was actually changed, so
            // top-ups made at their own rates are not overwritten by an unrelated edit.
            var moneyChanged = credit.PrincipalAmount != input.PrincipalAmount || credit.InterestRate != input.InterestRate;

            credit.PrincipalAmount = input.PrincipalAmount;
            credit.InterestRate = input.InterestRate;
            credit.TermMonths = input.TermMonths;
            credit.DueDate = input.DueDate;
            credit.Purpose = input.Purpose;
            credit.Notes = input.Notes;
            credit.RepaymentType = input.RepaymentType;

            if (moneyChanged)
            {
                credit.CalculateMoneyAmounts();
            }

            // A lender may mark a loan as active, suspended, defaulted or cancelled.
            // Other status values are ignored here; payments and the balance drive those.
            var manualStatuses = new[] { CreditStatus.Active, CreditStatus.Suspended, CreditStatus.Defaulted, CreditStatus.Cancelled };
            if (manualStatuses.Contains(input.Status))
            {
                credit.Status = input.Status;
            }

            // A balance of zero always closes the loan; a reopened loan becomes active again.
            if (credit.OutstandingAmount <= 0)
            {
                credit.Status = CreditStatus.PaidOff;
                credit.OutstandingAmount = 0;
            }
            else if (credit.Status == CreditStatus.PaidOff)
            {
                credit.Status = CreditStatus.Active;
            }

            await _creditRepository.UpdateAsync(credit);

            // Update borrower financials
            await UpdateBorrowerFinancialsAsync(credit.BorrowerId);

            return await GetAsync(id);
        }

        [Authorize(MoneyManagementPermissions.Credits.Edit)]
        public async Task<CreditDto> TopUpAsync(Guid id, TopUpCreditDto input)
        {
            EnsureNotAdmin();
            var credit = await _creditRepository.GetAsync(id);

            if (credit.Status == CreditStatus.Cancelled || credit.Status == CreditStatus.Defaulted)
            {
                throw new UserFriendlyException("A cancelled or defaulted loan cannot be topped up.");
            }

            var topUpRate = input.InterestRate!.Value;
            var extraInterest = credit.TopUp(input.Amount, topUpRate);

            if (input.DueDate.HasValue)
            {
                credit.DueDate = input.DueDate.Value;
            }

            // Record the top-up so the loan's history shows how the balance was reached.
            await _topUpRepository.InsertAsync(new CreditTopUp(
                GuidGenerator.Create(),
                credit.Id,
                input.Amount,
                topUpRate,
                extraInterest,
                Clock.Now,
                credit.OutstandingAmount,
                string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim()));

            await _creditRepository.UpdateAsync(credit);
            await UpdateBorrowerFinancialsAsync(credit.BorrowerId);

            return await GetAsync(id);
        }

        // Every top-up on a loan, oldest first.
        [Authorize(MoneyManagementPermissions.Credits.Default)]
        public async Task<ListResultDto<CreditTopUpDto>> GetTopUpsAsync(Guid id)
        {
            // Loads the loan first so a missing loan gives the usual not-found error.
            await _creditRepository.GetAsync(id);

            var queryable = await _topUpRepository.GetQueryableAsync();
            var topUps = await AsyncExecuter.ToListAsync(
                queryable.Where(t => t.CreditId == id).OrderBy(t => t.TopUpDate).ThenBy(t => t.CreationTime));

            return new ListResultDto<CreditTopUpDto>(topUps.Select(t => new CreditTopUpDto
            {
                Id = t.Id,
                CreditId = t.CreditId,
                Amount = t.Amount,
                InterestRate = t.InterestRate,
                InterestAmount = t.InterestAmount,
                TopUpDate = t.TopUpDate,
                OutstandingAfter = t.OutstandingAfter,
                Note = t.Note,
                CreationTime = t.CreationTime,
            }).ToList());
        }

        [Authorize(MoneyManagementPermissions.Credits.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            EnsureNotAdmin();
            var credit = await _creditRepository.GetAsync(id);

            // Check if credit has payments
            var hasPayments = await _paymentRepository.AnyAsync(p => p.CreditId == id);

            if (hasPayments)
            {
                throw new CannotDeleteMoneyWithPaymentsException(id);
            }

            var borrowerId = credit.BorrowerId;
            await _creditRepository.DeleteAsync(id);

            // Update borrower financials
            await UpdateBorrowerFinancialsAsync(borrowerId);
        }

        [Authorize(MoneyManagementPermissions.Credits.Default)]
        public async Task<PagedResultDto<OverdueCreditDto>> GetOverdueMoneysAsync(GetOverdueCreditsInput input)
        {
            var queryable = await _creditRepository.GetQueryableAsync();

            // Bad payers: loans still owing more than the admin-set grace period past their
            // expected payback date. This is how a borrower is rated; the loan's status is not involved.
            var graceDays = await SettingProvider.GetAsync(
                MoneyManagementSettings.BadPayers.GraceDays, MoneyManagementSettings.BadPayers.DefaultGraceDays);
            var highDays = await SettingProvider.GetAsync(
                MoneyManagementSettings.BadPayers.HighDays, MoneyManagementSettings.BadPayers.DefaultHighDays);
            var criticalDays = await SettingProvider.GetAsync(
                MoneyManagementSettings.BadPayers.CriticalDays, MoneyManagementSettings.BadPayers.DefaultCriticalDays);

            var today = DateTime.UtcNow.Date;
            var latestQualifyingDueDate = today.AddDays(-graceDays);
            queryable = queryable.Where(l =>
                l.OutstandingAmount > 0 &&
                l.DueDate < latestQualifyingDueDate &&
                l.Status != CreditStatus.PaidOff &&
                l.Status != CreditStatus.Cancelled &&
                l.Status != CreditStatus.Rejected);

            var credits = await AsyncExecuter.ToListAsync(queryable);

            if (input.MinDaysOverdue.HasValue)
            {
                credits = credits.Where(l =>
                    (DateTime.UtcNow.Date - l.DueDate.Date).Days >= input.MinDaysOverdue.Value
                ).ToList();
            }

            if (input.MaxDaysOverdue.HasValue)
            {
                credits = credits.Where(l =>
                    (DateTime.UtcNow.Date - l.DueDate.Date).Days <= input.MaxDaysOverdue.Value
                ).ToList();
            }

            var totalCount = credits.Count;

            // Apply sorting and paging
            credits = credits
                .OrderByDescending(l => (DateTime.UtcNow.Date - l.DueDate.Date).Days)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            // Map to DTO with additional info
            var dtos = new List<OverdueCreditDto>();
            foreach (var credit in credits)
            {
                var borrower = await _borrowerRepository.GetAsync(credit.BorrowerId);
                var lastPayment = (await _paymentRepository.GetQueryableAsync())
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefault(p => p.CreditId == credit.Id);

                var daysOverdue = (DateTime.UtcNow.Date - credit.DueDate.Date).Days;

                var dto = new OverdueCreditDto
                {
                    Id = credit.Id,
                    BorrowerId = credit.BorrowerId,
                    BorrowerName = borrower.FullName,
                    BorrowerPhone = borrower.PhoneNumber,
                    TotalAmount = credit.TotalAmount,
                    OutstandingAmount = credit.OutstandingAmount,
                    AmountPaid = credit.AmountPaid,
                    DueDate = credit.DueDate,
                    DaysOverdue = daysOverdue,
                    Severity = daysOverdue >= criticalDays ? OverdueSeverity.Critical
                        : daysOverdue >= highDays ? OverdueSeverity.High
                        : OverdueSeverity.Medium,
                    LastPaymentDate = lastPayment?.PaymentDate,
                    LastPaymentAmount = lastPayment?.Amount,
                    PaymentProgress = credit.TotalAmount > 0
                        ? (credit.AmountPaid / credit.TotalAmount) * 100
                        : 0
                };

                dtos.Add(dto);
            }

            return new PagedResultDto<OverdueCreditDto>(totalCount, dtos);
        }

        [Authorize(MoneyManagementPermissions.Credits.Approve)]
        public async Task<CreditDto> ApproveAsync(Guid id)
        {
            EnsureNotAdmin();
            var credit = await _creditRepository.GetAsync(id);
            credit.ApproveCredit();
            await _creditRepository.UpdateAsync(credit);

            return await GetAsync(id);
        }

        [Authorize(MoneyManagementPermissions.Credits.Approve)]
        public async Task<CreditDto> ActivateAsync(Guid id)
        {
            var credit = await _creditRepository.GetAsync(id);
            credit.ActivateCredit();
            await _creditRepository.UpdateAsync(credit);

            return await GetAsync(id);
        }

        [Authorize(MoneyManagementPermissions.Credits.WriteOff)]
        public async Task<CreditDto> SuspendAsync(Guid id)
        {
            var credit = await _creditRepository.GetAsync(id);

            if (credit.Status == CreditStatus.PaidOff)
            {
                throw new InvalidOperationException("Cannot suspend a paid off Money.");
            }

            credit.Status = CreditStatus.Suspended;
            await _creditRepository.UpdateAsync(credit);

            return await GetAsync(id);
        }

        [Authorize(MoneyManagementPermissions.Credits.Edit)]
        public async Task CheckAndUpdateOverdueMoneysAsync()
        {
            // Loans are no longer marked Overdue by date; move any that were back to Active.
            var overdueCredits = await _creditRepository.GetListAsync(c => c.Status == CreditStatus.Overdue);

            foreach (var credit in overdueCredits)
            {
                credit.CheckOverdueStatus();
                await _creditRepository.UpdateAsync(credit);
            }
        }

        [Authorize(MoneyManagementPermissions.Credits.Default)]
        public async Task<CreditCalculationDto> CalculateMoneyAsync(CreditCalculationInput input)
        {
            var interestAmount = input.PrincipalAmount * (input.InterestRate / 100);
            var totalAmount = input.PrincipalAmount + interestAmount;

            return new CreditCalculationDto
            {
                PrincipalAmount = input.PrincipalAmount,
                InterestRate = input.InterestRate,
                InterestAmount = interestAmount,
                TotalAmount = totalAmount,
                MonthlyPayment = CalculateMonthlyPayment(
                    totalAmount,
                    input.TermMonths)
            };
        }

        private decimal CalculateMonthlyPayment(decimal totalAmount, int months)
        {
            if (months <= 0) return totalAmount;
            return totalAmount / months;
        }

        private async Task UpdateBorrowerFinancialsAsync(Guid borrowerId)
        {
            var credits = await _creditRepository.GetListAsync(l => l.BorrowerId == borrowerId);

            var totalBorrowed = credits
                .Where(l => l.Status != CreditStatus.Rejected && l.Status != CreditStatus.Suspended)
                .Sum(l => l.TotalAmount);

            var outstanding = credits
                .Where(l => l.Status == CreditStatus.Active || l.Status == CreditStatus.Overdue)
                .Sum(l => l.OutstandingAmount);

            var paid = credits
                .Where(l => l.Status != CreditStatus.Rejected && l.Status != CreditStatus.Suspended)
                .Sum(l => l.AmountPaid);

            var defaultAmount = credits
                .Where(l => l.Status == CreditStatus.Defaulted)
                .Sum(l => l.OutstandingAmount);

            var defaultedMoneys = credits.Count(l => l.Status == CreditStatus.Defaulted);
            var totalMoneys = credits.Count(l => l.Status != CreditStatus.Rejected);

            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            borrower.UpdateFinancialSummary(totalBorrowed, outstanding, paid, defaultAmount, totalMoneys, defaultedMoneys);

            await _borrowerRepository.UpdateAsync(borrower);
        }

        /// <summary>
        /// Administrators see every lender's book but cannot change it:
        /// loans and payments belong to the lender who issued them.
        /// </summary>
        private void EnsureNotAdmin()
        {
            if (CurrentUser.IsInRole("admin"))
                throw new Volo.Abp.UserFriendlyException(
                    "Administrators can view loans and payments but not change them. They are managed by the lender who issued the loan.");
        }

    }
}