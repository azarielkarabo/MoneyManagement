using Money.Management.Entities;
using Money.Management.Enums;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace Money.Management.DomainServices
{
    public class LoanManager : DomainService
    {
        private readonly IRepository<Loan, Guid> _loanRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;

        public LoanManager(
            IRepository<Loan, Guid> loanRepository,
            IRepository<Borrower, Guid> borrowerRepository)
        {
            _loanRepository = loanRepository;
            _borrowerRepository = borrowerRepository;
        }

        public async Task<Loan> CreateAsync(
            Guid borrowerId,
            decimal principalAmount,
            decimal interestRate,
            DateTime startDate,
            DateTime? endDate = null,
            string notes = null)
        {
            await _borrowerRepository.GetAsync(borrowerId);

            if (principalAmount <= 0)
                throw new InvalidOperationException("Principal amount must be greater than 0.");

            if (interestRate < 0)
                throw new InvalidOperationException("Interest rate cannot be negative.");

            if (endDate.HasValue && endDate.Value <= startDate)
                throw new InvalidOperationException("End date must be after start date.");

            var loan = new Loan(
                GuidGenerator.Create(),
                borrowerId,
                principalAmount,
                interestRate,
                startDate,
                endDate,
                notes);

            return loan;
        }

        public async Task<LoanRepayment> MakeRepaymentAsync(Guid loanId, decimal amountPaid, DateTime paymentDate)
        {
            var loan = await _loanRepository.GetAsync(loanId);

            if (loan.LoanStatus == LoanStatus.Completed)
                throw new InvalidOperationException("Cannot make a repayment on a completed loan.");

            if (loan.LoanStatus == LoanStatus.Defaulted)
                throw new InvalidOperationException("Cannot make a repayment on a defaulted loan.");

            if (amountPaid <= 0)
                throw new InvalidOperationException("Repayment amount must be greater than 0.");

            var effectiveAmount = amountPaid > loan.OutstandingBalance
                ? loan.OutstandingBalance
                : amountPaid;

            loan.ApplyRepayment(effectiveAmount);
            await _loanRepository.UpdateAsync(loan);

            return new LoanRepayment(
                GuidGenerator.Create(),
                loanId,
                effectiveAmount,
                paymentDate);
        }
    }
}
