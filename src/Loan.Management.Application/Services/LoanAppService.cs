using AutoMapper.Internal.Mappers;
using Loan.Management.CustomExceptions;
using Loan.Management.DomainServices;
using Loan.Management.Entities;
using Loan.Management.Enums;
using Loan.Management.Permissions;
using Loan.Management.Services.Loans;
using Loan.Management.Services.Loans.Dtos;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Loan.Management.Services
{
    [Authorize(LoanManagementPermissions.Loans.Default)]
    public class LoanAppService : ApplicationService, ILoanAppService
    {
        private readonly IRepository<Entities.Loan, Guid> _loanRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IRepository<Payment, Guid> _paymentRepository;
        private readonly LoanManager _loanManager;
        private readonly BorrowerManager _borrowerManager;

        public LoanAppService(
            IRepository<Entities.Loan, Guid> loanRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            IRepository<Payment, Guid> paymentRepository,
            LoanManager loanManager,
            BorrowerManager borrowerManager)
        {
            _loanRepository = loanRepository;
            _borrowerRepository = borrowerRepository;
            _paymentRepository = paymentRepository;
            _loanManager = loanManager;
            _borrowerManager = borrowerManager;
        }

        public async Task<LoanDto> GetAsync(Guid id)
        {
            var loan = await _loanRepository.GetAsync(id);
            var dto = ObjectMapper.Map<Entities.Loan, LoanDto>(loan);

            // Calculate payment progress
            dto.PaymentProgress = dto.TotalAmount > 0
                ? (dto.AmountPaid / dto.TotalAmount) * 100
                : 0;

            return dto;
        }

        public async Task<PagedResultDto<LoanDto>> GetListAsync(GetLoansInput input)
        {
            var queryable = await _loanRepository.GetQueryableAsync();

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(l =>
                    l.BorrowerName.Contains(input.Filter) ||
                    l.Description.Contains(input.Filter));
            }

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
                queryable = queryable.Where(l => l.DisbursementDate >= input.FromDate.Value);
            }

            if (input.ToDate.HasValue)
            {
                queryable = queryable.Where(l => l.DisbursementDate <= input.ToDate.Value);
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
                queryable = queryable.Where(l => l.Status == LoanStatus.Overdue);
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply sorting
            //if (!string.IsNullOrWhiteSpace(input.Sorting))
            //{
            //    queryable = queryable.OrderBy(input.Sorting);
            //}
            //else
            //{
            queryable = queryable.OrderByDescending(l => l.DisbursementDate);
            //}

            queryable = queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var loans = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Entities.Loan>, List<LoanDto>>(loans);

            return new PagedResultDto<LoanDto>(totalCount, dtos);
        }

        [Authorize(LoanManagementPermissions.Loans.Create)]
        public async Task<LoanDto> CreateAsync(CreateLoanDto input)
        {
            // Use domain service to create loan (includes business logic and validation)
            var loan = await _loanManager.CreateAsync(
                input.BorrowerId,
                input.PrincipalAmount,
                input.InterestRate,
                input.DisbursementDate,
                input.DueDate,
                input.Description
            );

            await _loanRepository.InsertAsync(loan);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Update borrower financials
            await UpdateBorrowerFinancialsAsync(input.BorrowerId);

            return await GetAsync(loan.Id);
        }

        [Authorize(LoanManagementPermissions.Loans.Edit)]
        public async Task<LoanDto> UpdateAsync(Guid id, UpdateLoanDto input)
        {
            var loan = await _loanRepository.GetAsync(id);

            // Only allow updates if loan is not paid off
            if (loan.Status == LoanStatus.PaidOff)
            {
                throw new CannotUpdatePaidOffLoanException(id);
            }

            loan.PrincipalAmount = input.PrincipalAmount;
            loan.InterestRate = input.InterestRate;
            loan.DisbursementDate = input.DisbursementDate;
            loan.DueDate = input.DueDate;
            loan.Description = input.Description;
            loan.Status = input.Status;

            // Recalculate amounts
            loan.CalculateLoanAmounts();

            await _loanRepository.UpdateAsync(loan);

            // Update borrower financials
            await UpdateBorrowerFinancialsAsync(loan.BorrowerId);

            return await GetAsync(id);
        }

        [Authorize(LoanManagementPermissions.Loans.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            var loan = await _loanRepository.GetAsync(id);

            // Check if loan has payments
            var hasPayments = await _paymentRepository.AnyAsync(p => p.LoanId == id);

            if (hasPayments)
            {
                throw new CannotDeleteLoanWithPaymentsException(id);
            }

            var borrowerId = loan.BorrowerId;
            await _loanRepository.DeleteAsync(id);

            // Update borrower financials
            await UpdateBorrowerFinancialsAsync(borrowerId);
        }

        public async Task<PagedResultDto<OverdueLoanDto>> GetOverdueLoansAsync(
            GetOverdueLoansInput input)
        {
            var queryable = await _loanRepository.GetQueryableAsync();

            // Get only overdue loans
            queryable = queryable.Where(l => l.Status == LoanStatus.Overdue);

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(l => l.BorrowerName.Contains(input.Filter));
            }

            var loans = await AsyncExecuter.ToListAsync(queryable);

            // Apply severity filter (calculated property)
            if (input.Severity.HasValue)
            {
                loans = loans.Where(l => l.OverdueSeverity == input.Severity.Value).ToList();
            }

            if (input.MinDaysOverdue.HasValue)
            {
                loans = loans.Where(l => l.DaysOverdue >= input.MinDaysOverdue.Value).ToList();
            }

            if (input.MaxDaysOverdue.HasValue)
            {
                loans = loans.Where(l => l.DaysOverdue <= input.MaxDaysOverdue.Value).ToList();
            }

            var totalCount = loans.Count;

            // Apply sorting and paging
            loans = loans
                .OrderByDescending(l => l.DaysOverdue)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .ToList();

            // Map to DTO with additional info
            var dtos = new List<OverdueLoanDto>();
            foreach (var loan in loans)
            {
                var borrower = await _borrowerRepository.GetAsync(loan.BorrowerId);
                var lastPayment = (await _paymentRepository.GetQueryableAsync())
                    .OrderByDescending(p => p.PaymentDate)
                    .FirstOrDefault(p => p.LoanId == loan.Id);

                var dto = new OverdueLoanDto
                {
                    Id = loan.Id,
                    BorrowerId = loan.BorrowerId,
                    BorrowerName = loan.BorrowerName,
                    BorrowerPhone = borrower.User.PhoneNumber,
                    TotalAmount = loan.TotalAmount,
                    OutstandingAmount = loan.OutstandingAmount,
                    AmountPaid = loan.AmountPaid,
                    DueDate = loan.DueDate,
                    DaysOverdue = loan.DaysOverdue,
                    Severity = loan.OverdueSeverity,
                    LastPaymentDate = lastPayment?.PaymentDate,
                    LastPaymentAmount = lastPayment?.Amount,
                    PaymentProgress = loan.TotalAmount > 0
                        ? (loan.AmountPaid / loan.TotalAmount) * 100
                        : 0
                };

                dtos.Add(dto);
            }

            return new PagedResultDto<OverdueLoanDto>(totalCount, dtos);
        }

        [Authorize(LoanManagementPermissions.Loans.Approve)]
        public async Task<LoanDto> ApproveLoanAsync(Guid id)
        {
            var loan = await _loanRepository.GetAsync(id);
            loan.Status = LoanStatus.Active;
            await _loanRepository.UpdateAsync(loan);

            return await GetAsync(id);
        }

        [Authorize(LoanManagementPermissions.Loans.Edit)]
        public async Task<LoanDto> CancelLoanAsync(Guid id)
        {
            var loan = await _loanRepository.GetAsync(id);

            // Can only cancel if no payments made
            if (loan.AmountPaid > 0)
            {
                throw new CannotCancelLoanWithPaymentsException(id);
            }

            loan.Status = LoanStatus.Cancelled;
            await _loanRepository.UpdateAsync(loan);

            // Update borrower financials
            await UpdateBorrowerFinancialsAsync(loan.BorrowerId);

            return await GetAsync(id);
        }

        [Authorize(LoanManagementPermissions.Loans.WriteOff)]
        public async Task<LoanDto> WriteOffLoanAsync(Guid id)
        {
            var loan = await _loanRepository.GetAsync(id);
            loan.Status = LoanStatus.WriteOff;
            await _loanRepository.UpdateAsync(loan);

            // Update borrower financials
            await UpdateBorrowerFinancialsAsync(loan.BorrowerId);

            return await GetAsync(id);
        }

        public async Task CheckAndUpdateOverdueLoansAsync()
        {
            // Use domain service to check and update overdue loans
            await _loanManager.CheckAndUpdateOverdueLoansAsync();
        }

        public async Task<LoanCalculationDto> CalculateLoanAsync(LoanCalculationInput input)
        {
            var interestAmount = input.PrincipalAmount * (input.InterestRate / 100);
            var totalAmount = input.PrincipalAmount + interestAmount;

            return new LoanCalculationDto
            {
                PrincipalAmount = input.PrincipalAmount,
                InterestRate = input.InterestRate,
                InterestAmount = interestAmount,
                TotalAmount = totalAmount,
                MonthlyPayment = CalculateMonthlyPayment(
                    totalAmount,
                    input.DisbursementDate,
                    input.DueDate)
            };
        }

        private decimal CalculateMonthlyPayment(
            decimal totalAmount,
            DateTime disbursementDate,
            DateTime dueDate)
        {
            var months = ((dueDate.Year - disbursementDate.Year) * 12) +
                        dueDate.Month - disbursementDate.Month;

            if (months <= 0) return totalAmount;

            return totalAmount / months;
        }

        private async Task UpdateBorrowerFinancialsAsync(Guid borrowerId)
        {
            var loans = await _loanRepository.GetListAsync(l => l.BorrowerId == borrowerId);

            var totalBorrowed = loans
                .Where(l => l.Status != LoanStatus.Cancelled)
                .Sum(l => l.TotalAmount);

            var outstanding = loans
                .Where(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue)
                .Sum(l => l.OutstandingAmount);

            var paid = loans
                .Where(l => l.Status != LoanStatus.Cancelled)
                .Sum(l => l.AmountPaid);

            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            borrower.UpdateFinancialSummary(totalBorrowed, outstanding, paid);

            await _borrowerRepository.UpdateAsync(borrower);
        }
    }
}
