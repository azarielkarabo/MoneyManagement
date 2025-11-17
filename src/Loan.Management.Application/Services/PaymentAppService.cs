using AutoMapper.Internal.Mappers;
using Loan.Management.CustomExceptions;
using Loan.Management.DomainServices;
using Loan.Management.Entities;
using Loan.Management.Enums;
using Loan.Management.Permissions;
using Loan.Management.Services.Payments;
using Loan.Management.Services.Payments.Dtos;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Loan.Management.Services
{
    [Authorize(LoanManagementPermissions.Payments.Default)]
    public class PaymentAppService : ApplicationService, IPaymentAppService
    {
        private readonly IRepository<Payment, Guid> _paymentRepository;
        private readonly IRepository<Entities.Loan, Guid> _loanRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly LoanManager _loanManager;
        private readonly BorrowerManager _borrowerManager;

        public PaymentAppService(
            IRepository<Payment, Guid> paymentRepository,
            IRepository<Entities.Loan, Guid> loanRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            LoanManager loanManager,
            BorrowerManager borrowerManager)
        {
            _paymentRepository = paymentRepository;
            _loanRepository = loanRepository;
            _borrowerRepository = borrowerRepository;
            _loanManager = loanManager;
            _borrowerManager = borrowerManager;
        }

        public async Task<PaymentDto> GetAsync(Guid id)
        {
            var payment = await _paymentRepository.GetAsync(id);
            return ObjectMapper.Map<Payment, PaymentDto>(payment);
        }

        public async Task<PagedResultDto<PaymentDto>> GetListAsync(GetPaymentsInput input)
        {
            var queryable = await _paymentRepository.GetQueryableAsync();

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(p =>
                    p.BorrowerName.Contains(input.Filter) ||
                    (p.Description != null && p.Description.Contains(input.Filter)));
            }

            if (input.BorrowerId.HasValue)
            {
                queryable = queryable.Where(p => p.BorrowerId == input.BorrowerId.Value);
            }

            if (input.LoanId.HasValue)
            {
                queryable = queryable.Where(p => p.LoanId == input.LoanId.Value);
            }

            if (input.Method.HasValue)
            {
                queryable = queryable.Where(p => p.Method == input.Method.Value);
            }

            if (input.FromDate.HasValue)
            {
                queryable = queryable.Where(p => p.PaymentDate >= input.FromDate.Value);
            }

            if (input.ToDate.HasValue)
            {
                queryable = queryable.Where(p => p.PaymentDate <= input.ToDate.Value);
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            //// Apply sorting
            //if (!string.IsNullOrWhiteSpace(input.Sorting))
            //{
            //    queryable = queryable.OrderBy(input.Sorting);
            //}
            //else
            //{
                queryable = queryable.OrderByDescending(p => p.PaymentDate);
            //}

            queryable = queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var payments = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Payment>, List<PaymentDto>>(payments);

            return new PagedResultDto<PaymentDto>(totalCount, dtos);
        }

        [Authorize(LoanManagementPermissions.Payments.Create)]
        public async Task<PaymentDto> CreateAsync(CreatePaymentDto input)
        {
            var loan = await _loanRepository.GetAsync(input.LoanId);

            // Validate payment amount doesn't exceed outstanding
            if (input.Amount > loan.OutstandingAmount)
            {
                throw new PaymentExceedsOutstandingException(
                    input.Amount,
                    loan.OutstandingAmount);
            }

            // Validate payment amount is positive
            if (input.Amount <= 0)
            {
                throw new InvalidPaymentAmountException(input.Amount);
            }

            // Cannot make payment on cancelled loan
            if (loan.Status == LoanStatus.Cancelled)
            {
                throw new CannotPayCancelledLoanException(input.LoanId);
            }

            var previousOutstanding = loan.OutstandingAmount;

            // Create payment entity
            var payment = new Payment(
                GuidGenerator.Create(),
                input.LoanId,
                loan.BorrowerId,
                loan.BorrowerName,
                input.Amount,
                input.PaymentDate,
                previousOutstanding,
                input.Method,
                input.Description
            );

            await _paymentRepository.InsertAsync(payment);

            // Use domain service to record payment on loan
            await _loanManager.RecordPaymentAsync(input.LoanId, input.Amount);

            // Update borrower financials using domain service
            await _borrowerManager.UpdateFinancialSummaryAsync(loan.BorrowerId);

            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetAsync(payment.Id);
        }

        [Authorize(LoanManagementPermissions.Payments.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            var payment = await _paymentRepository.GetAsync(id);
            var loan = await _loanRepository.GetAsync(payment.LoanId);

            // Reverse payment on loan
            loan.AmountPaid -= payment.Amount;
            loan.OutstandingAmount += payment.Amount;

            // Update loan status
            if (loan.OutstandingAmount > 0)
            {
                if (DateTime.UtcNow > loan.DueDate)
                {
                    loan.Status = LoanStatus.Overdue;
                }
                else
                {
                    loan.Status = LoanStatus.Active;
                }
            }

            await _loanRepository.UpdateAsync(loan);
            await _paymentRepository.DeleteAsync(id);

            // Update borrower financials using domain service
            await _borrowerManager.UpdateFinancialSummaryAsync(loan.BorrowerId);
        }

        public async Task<List<PaymentsByBorrowerDto>> GetPaymentsByBorrowerAsync(
            GetPaymentsInput input)
        {
            var queryable = await _paymentRepository.GetQueryableAsync();

            // Apply date filters
            if (input.FromDate.HasValue)
            {
                queryable = queryable.Where(p => p.PaymentDate >= input.FromDate.Value);
            }

            if (input.ToDate.HasValue)
            {
                queryable = queryable.Where(p => p.PaymentDate <= input.ToDate.Value);
            }

            var payments = await AsyncExecuter.ToListAsync(queryable);

            // Group by borrower
            var groupedPayments = payments
                .GroupBy(p => p.BorrowerId)
                .Select(g => new PaymentsByBorrowerDto
                {
                    BorrowerId = g.Key,
                    BorrowerName = g.First().BorrowerName,
                    TotalPaid = g.Sum(p => p.Amount),
                    PaymentCount = g.Count(),
                    LastPaymentDate = g.Max(p => p.PaymentDate),
                    Payments = ObjectMapper.Map<List<Payment>, List<PaymentDto>>(
                        g.OrderByDescending(p => p.PaymentDate).ToList())
                })
                .OrderByDescending(g => g.TotalPaid)
                .ToList();

            return groupedPayments;
        }

        [Authorize(LoanManagementPermissions.Payments.Approve)]
        public async Task<PaymentDto> ApprovePaymentAsync(Guid id)
        {
            // This could be used if you have a payment approval workflow
            // For example, payments might need manager approval before they're finalized
            var payment = await _paymentRepository.GetAsync(id);

            // Add approval logic here if needed
            // Example: payment.IsApproved = true;
            // await _paymentRepository.UpdateAsync(payment);

            return ObjectMapper.Map<Payment, PaymentDto>(payment);
        }

        public async Task<decimal> GetTotalPaymentsAsync(
            Guid? borrowerId = null,
            Guid? loanId = null)
        {
            var queryable = await _paymentRepository.GetQueryableAsync();

            if (borrowerId.HasValue)
            {
                queryable = queryable.Where(p => p.BorrowerId == borrowerId.Value);
            }

            if (loanId.HasValue)
            {
                queryable = queryable.Where(p => p.LoanId == loanId.Value);
            }

            var payments = await AsyncExecuter.ToListAsync(queryable);
            var total = payments.Sum(p => p.Amount);

            return total;
        }

        public async Task<List<PaymentDto>> GetRecentPaymentsAsync(int count = 10)
        {
            var queryable = await _paymentRepository.GetQueryableAsync();

            queryable = queryable
                .OrderByDescending(p => p.PaymentDate)
                .Take(count);

            var payments = await AsyncExecuter.ToListAsync(queryable);
            return ObjectMapper.Map<List<Payment>, List<PaymentDto>>(payments);
        }
    }
}
