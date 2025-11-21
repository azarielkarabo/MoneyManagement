using Loan.Management.CustomExceptions;
using Loan.Management.Entities;
using Loan.Management.Enums;
using Loan.Management.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;

namespace Loan.Management.DomainServices
{
    public class CreditManager : DomainService
    {
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly ILocalEventBus _localEventBus;

        public CreditManager(
            IRepository<Credit, Guid> creditRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            ILocalEventBus localEventBus)
        {
            _creditRepository = creditRepository;
            _borrowerRepository = borrowerRepository;
            _localEventBus = localEventBus;
        }

        public async Task<Credit> CreateAsync(
            Guid borrowerId,
            Guid lenderId,
            decimal principalAmount,
            decimal interestRate,
            int termMonths,
            DateTime dueDate,
            string purpose = null)
        {
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            // Validate dates
            var today = DateTime.UtcNow;
            if (dueDate <= today)
            {
                throw new InvalidLoanDatesException();
            }

            // Validate principal amount
            if (principalAmount <= 0)
            {
                throw new InvalidPrincipalAmountException(principalAmount);
            }

            // Validate term months
            if (termMonths <= 0)
            {
                throw new InvalidOperationException("Term months must be greater than 0.");
            }

            // Validate interest rate
            if (interestRate < 0)
            {
                throw new InvalidOperationException("Interest rate cannot be negative.");
            }

            var credit = new Credit(
                GuidGenerator.Create(),
                borrowerId,
                lenderId,
                principalAmount,
                interestRate,
                termMonths,
                dueDate,
                purpose
            );

            // Publish event
            await _localEventBus.PublishAsync(new CreditCreatedEvent(credit));

            return credit;
        }

        public async Task RecordPaymentAsync(Guid creditId, decimal amount)
        {
            var credit = await _creditRepository.GetAsync(creditId);

            if (amount <= 0)
            {
                throw new InvalidPaymentAmountException(amount);
            }

            if (amount > credit.OutstandingAmount)
            {
                throw new PaymentExceedsOutstandingException(
                    amount,
                    credit.OutstandingAmount
                );
            }

            if (credit.Status == CreditStatus.Suspended || credit.Status == CreditStatus.Rejected)
            {
                throw new InvalidOperationException($"Cannot record payment on a {credit.Status} credit.");
            }

            credit.RecordPayment(amount);
            await _creditRepository.UpdateAsync(credit);

            // Publish payment recorded event
            await _localEventBus.PublishAsync(new CreditPaymentRecordedEvent(credit, amount));
        }

        public async Task CheckAndUpdateOverdueLoansAsync()
        {
            var activeLoans = await _creditRepository
                .GetListAsync(l =>
                    (l.Status == CreditStatus.Active || l.Status == CreditStatus.Approved) &&
                    l.OutstandingAmount > 0);

            var updatedLoans = new List<Credit>();

            foreach (var credit in activeLoans)
            {
                credit.CheckOverdueStatus();

                if (credit.Status == CreditStatus.Overdue)
                {
                    updatedLoans.Add(credit);
                }
            }

            if (updatedLoans.Any())
            {
                await _creditRepository.UpdateManyAsync(updatedLoans);
            }
        }

        public async Task<decimal> CalculateTotalOutstandingAsync(Guid borrowerId)
        {
            var credits = await _creditRepository.GetListAsync(l =>
                l.BorrowerId == borrowerId &&
                (l.Status == CreditStatus.Active || l.Status == CreditStatus.Overdue));

            return credits.Sum(l => l.OutstandingAmount);
        }

        public async Task<int> GetActiveLoansCountAsync(Guid borrowerId)
        {
            return await _creditRepository.CountAsync(l =>
                l.BorrowerId == borrowerId &&
                (l.Status == CreditStatus.Active || l.Status == CreditStatus.Overdue));
        }

        public async Task<int> GetTotalLoansCountAsync(Guid borrowerId)
        {
            return await _creditRepository.CountAsync(l =>
                l.BorrowerId == borrowerId &&
                l.Status != CreditStatus.Rejected);
        }

        public async Task<decimal> GetTotalBorrowedAsync(Guid borrowerId)
        {
            var credits = await _creditRepository.GetListAsync(l =>
                l.BorrowerId == borrowerId &&
                l.Status != CreditStatus.Rejected);

            return credits.Sum(l => l.TotalAmount);
        }
    }
}