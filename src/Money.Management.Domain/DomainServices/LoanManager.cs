using Money.Management.Entities;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Money.Management.DomainServices
{
    /// <summary>
    /// Orchestrates operations that require cross-aggregate or repository access.
    /// Business invariants that concern only the Loan aggregate live on <see cref="Loan"/>
    /// itself; this service handles the coordination layer.
    /// </summary>
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

        /// <summary>
        /// Creates a new loan for the given borrower. Verifies the borrower exists
        /// before constructing the aggregate so the domain constructor can stay
        /// infrastructure-free.
        /// </summary>
        public async Task<Loan> CreateAsync(
            Guid borrowerId,
            decimal principalAmount,
            decimal interestRate,
            DateTime startDate,
            DateTime? endDate = null,
            string notes = null)
        {
            // Cross-aggregate check — borrower must exist
            await _borrowerRepository.GetAsync(borrowerId);

            // Loan constructor enforces all term-level invariants
            return new Loan(
                GuidGenerator.Create(),
                borrowerId,
                principalAmount,
                interestRate,
                startDate,
                endDate,
                notes);
        }

        /// <summary>
        /// Approves a pending loan and persists the state change.
        /// </summary>
        public async Task<Loan> ApproveAsync(Guid loanId)
        {
            var loan = await _loanRepository.GetAsync(loanId);
            loan.Approve(); // throws CannotApproveLoanException if already active/completed
            await _loanRepository.UpdateAsync(loan);
            return loan;
        }

        /// <summary>
        /// Records a repayment against an active loan. The loan aggregate creates the
        /// <see cref="LoanRepayment"/> internally; this service persists both.
        /// </summary>
        public async Task<LoanRepayment> MakeRepaymentAsync(
            Guid loanId,
            decimal amount,
            DateTime paymentDate,
            string notes = null)
        {
            // includeDetails: true loads the _repayments collection so the entity
            // can correctly check its own state and add to the list.
            var loan = await _loanRepository.GetAsync(loanId, includeDetails: true);

            // All validation + state transition lives on the entity
            var repayment = loan.MakeRepayment(
                GuidGenerator.Create(), amount, paymentDate, notes);

            await _loanRepository.UpdateAsync(loan);
            return repayment;
        }

        /// <summary>
        /// Marks an active loan as defaulted and persists the change.
        /// </summary>
        public async Task MarkAsDefaultedAsync(Guid loanId)
        {
            var loan = await _loanRepository.GetAsync(loanId);
            loan.MarkAsDefaulted(); // throws LoanNotActiveException if not Active
            await _loanRepository.UpdateAsync(loan);
        }
    }
}
