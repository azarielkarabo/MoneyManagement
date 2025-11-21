using Loan.Management.CustomExceptions;
using Loan.Management.DomainServices;
using Loan.Management.Entities;
using Loan.Management.Enums;
using Loan.Management.Services.credits;
using Loan.Management.Services.credits.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Loan.Management.Services
{
    public class CreditAppService : ApplicationService, ICreditAppService
    {
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IRepository<Payment, Guid> _paymentRepository;

        public CreditAppService(
            IRepository<Credit, Guid> creditRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            IRepository<Payment, Guid> paymentRepository,
            CreditManager loanManager,
            BorrowerManager borrowerManager)
        {
            _creditRepository = creditRepository;
            _borrowerRepository = borrowerRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<CreditDto> GetAsync(Guid id)
        {
            var credit = await _creditRepository.GetAsync(id);
            var dto = ObjectMapper.Map<Credit, CreditDto>(credit);

            // Calculate payment progress
            dto.PaymentProgress = dto.TotalAmount > 0
                ? (dto.AmountPaid / dto.TotalAmount) * 100
                : 0;

            return dto;
        }

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

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable.OrderByDescending(l => l.IssuedDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var credits = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Credit>, List<CreditDto>>(credits);

            return new PagedResultDto<CreditDto>(totalCount, dtos);
        }

        public async Task<CreditDto> CreateAsync(CreateCreditDto input)
        {
            var credit = new Credit(
                GuidGenerator.Create(),
                input.BorrowerId,
                CurrentUser.Id.Value,
                input.PrincipalAmount,
                input.InterestRate,
                input.TermMonths,
                input.DueDate,
                input.Purpose
            );

            await _creditRepository.InsertAsync(credit);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Update borrower financials
            await UpdateBorrowerFinancialsAsync(input.BorrowerId);

            return await GetAsync(credit.Id);
        }

        public async Task<CreditDto> UpdateAsync(Guid id, UpdateCreditDto input)
        {
            var credit = await _creditRepository.GetAsync(id);

            // Only allow updates if credit is not paid off
            if (credit.Status == CreditStatus.PaidOff)
            {
                throw new CannotUpdatePaidOffLoanException(id);
            }

            credit.PrincipalAmount = input.PrincipalAmount;
            credit.InterestRate = input.InterestRate;
            credit.TermMonths = input.TermMonths;
            credit.DueDate = input.DueDate;
            credit.Purpose = input.Purpose;

            // Recalculate amounts
            credit.CalculateLoanAmounts();

            await _creditRepository.UpdateAsync(credit);

            // Update borrower financials
            await UpdateBorrowerFinancialsAsync(credit.BorrowerId);

            return await GetAsync(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var credit = await _creditRepository.GetAsync(id);

            // Check if credit has payments
            var hasPayments = await _paymentRepository.AnyAsync(p => p.CreditId == id);

            if (hasPayments)
            {
                throw new CannotDeleteLoanWithPaymentsException(id);
            }

            var borrowerId = credit.BorrowerId;
            await _creditRepository.DeleteAsync(id);

            // Update borrower financials
            await UpdateBorrowerFinancialsAsync(borrowerId);
        }

        public async Task<PagedResultDto<OverdueCreditDto>> GetOverdueLoansAsync(GetOverdueCreditsInput input)
        {
            var queryable = await _creditRepository.GetQueryableAsync();

            // Get only overdue credits
            queryable = queryable.Where(l => l.Status == CreditStatus.Overdue);

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

        public async Task<CreditDto> ApproveAsync(Guid id)
        {
            var credit = await _creditRepository.GetAsync(id);
            credit.ApproveCredit();
            await _creditRepository.UpdateAsync(credit);

            return await GetAsync(id);
        }

        public async Task<CreditDto> ActivateAsync(Guid id)
        {
            var credit = await _creditRepository.GetAsync(id);
            credit.ActivateCredit();
            await _creditRepository.UpdateAsync(credit);

            return await GetAsync(id);
        }

        public async Task<CreditDto> SuspendAsync(Guid id)
        {
            var credit = await _creditRepository.GetAsync(id);

            if (credit.Status == CreditStatus.PaidOff)
            {
                throw new InvalidOperationException("Cannot suspend a paid off loan.");
            }

            credit.Status = CreditStatus.Suspended;
            await _creditRepository.UpdateAsync(credit);

            return await GetAsync(id);
        }

        public async Task CheckAndUpdateOverdueLoansAsync()
        {
            var activeCredits = await _creditRepository.GetListAsync(
                c => c.Status == CreditStatus.Active || c.Status == CreditStatus.Approved
            );

            foreach (var credit in activeCredits)
            {
                credit.CheckOverdueStatus();
                await _creditRepository.UpdateAsync(credit);
            }
        }

        public async Task<CreditCalculationDto> CalculateLoanAsync(CreditCalculationInput input)
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

            var defaultedLoans = credits.Count(l => l.Status == CreditStatus.Defaulted);
            var totalLoans = credits.Count(l => l.Status != CreditStatus.Rejected);

            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            borrower.UpdateFinancialSummary(totalBorrowed, outstanding, paid, defaultAmount, totalLoans, defaultedLoans);

            await _borrowerRepository.UpdateAsync(borrower);
        }
    }
}