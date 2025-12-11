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
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Users;

namespace Loan.Management.Services
{
    public class PaymentAppService : ApplicationService, IPaymentAppService
    {
        private readonly IRepository<Payment, Guid> _paymentRepository;
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly CreditManager _creditManager;
        private readonly BorrowerManager _borrowerManager;
        private readonly LenderManager _lenderManager;
        private readonly ICurrentUser _currentUser;

        public PaymentAppService(
            IRepository<Payment, Guid> paymentRepository,
            IRepository<Credit, Guid> creditRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            CreditManager creditManager,
            BorrowerManager borrowerManager,
            LenderManager lenderManager,
            ICurrentUser currentUser)
        {
            _paymentRepository = paymentRepository;
            _creditRepository = creditRepository;
            _borrowerRepository = borrowerRepository;
            _creditManager = creditManager;
            _borrowerManager = borrowerManager;
            _lenderManager = lenderManager;
            _currentUser = currentUser;
        }

        private async Task<Guid> GetCurrentLenderIdAsync()
        {
            var lender = await _lenderManager.GetCurrentLenderAsync();
            return lender.Id;
        }

        public async Task<PaymentDto> GetAsync(Guid id)
        {
            var payment = await _paymentRepository.GetAsync(id);
            return ObjectMapper.Map<Payment, PaymentDto>(payment);
        }

        public async Task<PagedResultDto<PaymentDto>> GetListAsync(GetPaymentsInput input)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var queryable = await _paymentRepository.GetQueryableAsync();

            // Get credits for this lender to filter payments
            var lenderCreditIds = (await _creditRepository.GetListAsync(c => c.LenderId == lenderId))
                .Select(c => c.Id)
                .ToList();

            queryable = queryable.Where(p => lenderCreditIds.Contains(p.CreditId));

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(p =>
                    (p.Description != null && p.Description.Contains(input.Filter)));
            }

            if (input.BorrowerId.HasValue)
            {
                queryable = queryable.Where(p => p.BorrowerId == input.BorrowerId.Value);
            }

            if (input.CreditId.HasValue)
            {
                queryable = queryable.Where(p => p.CreditId == input.CreditId.Value);
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

            queryable = queryable
                .OrderByDescending(p => p.PaymentDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var payments = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Payment>, List<PaymentDto>>(payments);

            return new PagedResultDto<PaymentDto>(totalCount, dtos);
        }

        public async Task<PaymentDto> CreateAsync(CreatePaymentDto input)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var credit = await _creditRepository.GetAsync(input.CreditId);

            // Verify credit belongs to current lender
            if (credit.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to record payment for this credit.");
            }

            // Validate payment amount doesn't exceed outstanding
            if (input.Amount > credit.OutstandingAmount)
            {
                throw new PaymentExceedsOutstandingException(
                    input.Amount,
                    credit.OutstandingAmount);
            }

            // Validate payment amount is positive
            if (input.Amount <= 0)
            {
                throw new InvalidPaymentAmountException(input.Amount);
            }

            // Cannot make payment on suspended or rejected credit
            if (credit.Status == CreditStatus.Suspended || credit.Status == CreditStatus.Rejected)
            {
                throw new InvalidOperationException($"Cannot record payment on a {credit.Status} credit.");
            }

            // Get borrower for reference
            var borrower = await _borrowerRepository.GetAsync(credit.BorrowerId);

            // Create payment entity
            var payment = new Payment(
                GuidGenerator.Create(),
                lenderId,
                input.CreditId,
                credit.BorrowerId,
                borrower.FullName,
                input.Amount,
                input.PaymentDate,
                credit.OutstandingAmount,
                input.Method,
                input.Description
            );

            await _paymentRepository.InsertAsync(payment);

            // Use domain service to record payment on credit
            await _creditManager.RecordPaymentAsync(input.CreditId, input.Amount);

            // Update borrower financials using domain service
            await _borrowerManager.UpdateFinancialSummaryAsync(credit.BorrowerId);

            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetAsync(payment.Id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var payment = await _paymentRepository.GetAsync(id);
            var credit = await _creditRepository.GetAsync(payment.CreditId);

            // Verify credit belongs to current lender
            if (credit.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this payment.");
            }

            // Reverse payment on credit
            credit.AmountPaid -= payment.Amount;
            credit.OutstandingAmount += payment.Amount;

            // Update credit status
            if (credit.OutstandingAmount > 0)
            {
                if (DateTime.UtcNow > credit.DueDate)
                {
                    credit.Status = CreditStatus.Overdue;
                }
                else
                {
                    credit.Status = CreditStatus.Active;
                }
            }

            await _creditRepository.UpdateAsync(credit);
            await _paymentRepository.DeleteAsync(id);

            // Update borrower financials using domain service
            await _borrowerManager.UpdateFinancialSummaryAsync(credit.BorrowerId);
        }

        public async Task<List<PaymentsByBorrowerDto>> GetPaymentsByBorrowerAsync(
            GetPaymentsInput input)
        {
            var lenderId = await GetCurrentLenderIdAsync();

            // Get all credits for this lender
            var lenderCredits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);
            var lenderCreditIds = lenderCredits.Select(c => c.Id).ToList();

            var queryable = await _paymentRepository.GetQueryableAsync();

            // Filter by lender's credits
            queryable = queryable.Where(p => lenderCreditIds.Contains(p.CreditId));

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

        public async Task<PaymentDto> ApprovePaymentAsync(Guid id)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var payment = await _paymentRepository.GetAsync(id);
            var credit = await _creditRepository.GetAsync(payment.CreditId);

            // Verify credit belongs to current lender
            if (credit.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to approve this payment.");
            }

            // Add approval logic here if needed
            return ObjectMapper.Map<Payment, PaymentDto>(payment);
        }

        public async Task<decimal> GetTotalPaymentsAsync(
            Guid? borrowerId = null,
            Guid? creditId = null)
        {
            var lenderId = await GetCurrentLenderIdAsync();

            var queryable = await _paymentRepository.GetQueryableAsync();

            // Get all credits for this lender
            var lenderCreditIds = (await _creditRepository.GetListAsync(c => c.LenderId == lenderId))
                .Select(c => c.Id)
                .ToList();

            queryable = queryable.Where(p => lenderCreditIds.Contains(p.CreditId));

            if (borrowerId.HasValue)
            {
                queryable = queryable.Where(p => p.BorrowerId == borrowerId.Value);
            }

            if (creditId.HasValue)
            {
                queryable = queryable.Where(p => p.CreditId == creditId.Value);
            }

            var payments = await AsyncExecuter.ToListAsync(queryable);
            return payments.Sum(p => p.Amount);
        }

        public async Task<List<PaymentDto>> GetRecentPaymentsAsync(int count = 10)
        {
            var lenderId = await GetCurrentLenderIdAsync();

            // Get all credits for this lender
            var lenderCreditIds = (await _creditRepository.GetListAsync(c => c.LenderId == lenderId))
                .Select(c => c.Id)
                .ToList();

            var queryable = await _paymentRepository.GetQueryableAsync();

            queryable = queryable
                .Where(p => lenderCreditIds.Contains(p.CreditId))
                .OrderByDescending(p => p.PaymentDate)
                .Take(count);

            var payments = await AsyncExecuter.ToListAsync(queryable);
            return ObjectMapper.Map<List<Payment>, List<PaymentDto>>(payments);
        }
    }
}