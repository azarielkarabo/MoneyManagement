using Loan.Management.CustomExceptions;
using Loan.Management.Entities;
using Loan.Management.Enums;
using Loan.Management.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.EventBus.Local;
using Volo.Abp.Guids;

namespace Loan.Management.DomainServices
{
    public class LoanManager : DomainService
    {
        private readonly IRepository<Entities.Loan, Guid> _loanRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly ILocalEventBus _localEventBus;

        public LoanManager(
            IRepository<Entities.Loan, Guid> loanRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            ILocalEventBus localEventBus)
        {
            _loanRepository = loanRepository;
            _borrowerRepository = borrowerRepository;
            _localEventBus = localEventBus;
        }

        public async Task<Entities.Loan> CreateAsync(
            Guid borrowerId,
            decimal principalAmount,
            decimal interestRate,
            DateTime disbursementDate,
            DateTime dueDate,
            string description = null)
        {
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            // Validate dates
            if (dueDate <= disbursementDate)
            {
                throw new InvalidLoanDatesException();
            }

            // Validate disbursement date is not in the far past
            if (disbursementDate < DateTime.UtcNow.AddYears(-1))
            {
                throw new InvalidDisbursementDateException(disbursementDate);
            }

            // Validate principal amount
            if (principalAmount <= 0)
            {
                throw new InvalidPrincipalAmountException(principalAmount);
            }

            var loan = new Entities.Loan(
                GuidGenerator.Create(),
                borrowerId,
                borrower.User.Name,
                principalAmount,
                interestRate,
                disbursementDate,
                dueDate,
                description
            );

            // Publish event
            await _localEventBus.PublishAsync(new LoanCreatedEvent(loan));

            return loan;
        }

        public async Task RecordPaymentAsync(Guid loanId, decimal amount)
        {
            var loan = await _loanRepository.GetAsync(loanId);

            if (amount <= 0)
            {
                throw new InvalidPaymentAmountException(amount);
            }

            if (amount > loan.OutstandingAmount)
            {
                throw new PaymentExceedsOutstandingException(
                    amount,
                    loan.OutstandingAmount
                );
            }

            if (loan.Status == LoanStatus.Cancelled)
            {
                throw new CannotPayCancelledLoanException(loanId);
            }

            loan.RecordPayment(amount);
            await _loanRepository.UpdateAsync(loan);
        }

        public async Task CheckAndUpdateOverdueLoansAsync()
        {
            var activeLoans = await _loanRepository
                .GetListAsync(l =>
                    l.Status == LoanStatus.Active &&
                    l.OutstandingAmount > 0);

            var today = DateTime.UtcNow.Date;
            var updatedLoans = new List<Entities.Loan>();

            foreach (var loan in activeLoans)
            {
                if (loan.DueDate.Date < today)
                {
                    loan.CheckOverdueStatus();
                    updatedLoans.Add(loan);
                }
            }

            if (updatedLoans.Any())
            {
                await _loanRepository.UpdateManyAsync(updatedLoans);
            }
        }

        public async Task<decimal> CalculateTotalOutstandingAsync(Guid borrowerId)
        {
            var loans = await _loanRepository.GetListAsync(l =>
                l.BorrowerId == borrowerId &&
                (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));

            return loans.Sum(l => l.OutstandingAmount);
        }

        public async Task<int> GetActiveLoansCountAsync(Guid borrowerId)
        {
            return await _loanRepository.CountAsync(l =>
                l.BorrowerId == borrowerId &&
                (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));
        }
    }

}
