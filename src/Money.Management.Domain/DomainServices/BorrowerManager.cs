using Money.Management.CustomExceptions;
using Money.Management.Entities;
using Money.Management.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Users;

namespace Money.Management.DomainServices
{
    public class BorrowerManager : DomainService
    {
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly ICurrentUser _currentUser;

        public BorrowerManager(
            IRepository<Borrower, Guid> borrowerRepository,
            IRepository<Credit, Guid> creditRepository,
            ICurrentUser currentUser)
        {
            _borrowerRepository = borrowerRepository;
            _creditRepository = creditRepository;
            _currentUser = currentUser;
        }

        /// <summary>
        /// Resolves the Borrower record that the currently authenticated user is allowed to view
        /// as "their own" — first by a direct UserId link (set when a BorrowerApplication is
        /// approved), falling back to matching the current user's email against a Borrower's
        /// EmailAddress for borrowers a lender created directly (no linked login account).
        /// </summary>
        public async Task<Borrower> GetCurrentBorrowerAsync()
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new InvalidOperationException("User is not authenticated.");
            }

            var currentUserId = _currentUser.Id;
            if (currentUserId.HasValue)
            {
                var byUserId = await _borrowerRepository.FirstOrDefaultAsync(b => b.UserId == currentUserId.Value);
                if (byUserId != null)
                {
                    return byUserId;
                }
            }

            var email = _currentUser.Email;
            if (!string.IsNullOrWhiteSpace(email))
            {
                var byEmail = await _borrowerRepository.FirstOrDefaultAsync(b => b.EmailAddress == email);
                if (byEmail != null)
                {
                    return byEmail;
                }
            }

            throw new InvalidOperationException("No borrower profile found for the current user.");
        }

        public async Task<Borrower> CreateAsync(
            Guid lenderId,
            string firstName,
            string lastName,
            string idNumber,
            IdentificationType idType,
            string email,
            string phoneNumber,
            string address,
            decimal monthlyIncome)
        {
            // Check for duplicate ID number
            var existingBorrower = await _borrowerRepository
                .FirstOrDefaultAsync(b => b.IdNumber == idNumber);

            if (existingBorrower != null)
            {
                throw new BorrowerAlreadyExistsException(idNumber);
            }

            var borrower = new Borrower(
                GuidGenerator.Create(),
                lenderId,
                firstName,
                lastName,
                idNumber,
                idType,
                email,
                phoneNumber,
                address,
                monthlyIncome
            );

            return borrower;
        }

        public async Task UpdateFinancialSummaryAsync(Guid borrowerId)
        {
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
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

            borrower.UpdateFinancialSummary(totalBorrowed, outstanding, paid, defaultAmount, totalMoneys, defaultedMoneys);

            await _borrowerRepository.UpdateAsync(borrower);
        }

        public async Task SuspendBorrowerAsync(Guid borrowerId, string reason)
        {
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            borrower.Suspend(reason);
            await _borrowerRepository.UpdateAsync(borrower);
        }

        public async Task BlacklistBorrowerAsync(Guid borrowerId, string reason)
        {
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            borrower.Blacklist(reason);
            await _borrowerRepository.UpdateAsync(borrower);
        }

        public async Task<decimal> GetDebtToIncomeRatioAsync(Guid borrowerId)
        {
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            if (borrower.MonthlyIncome <= 0)
            {
                throw new InvalidOperationException("Cannot calculate debt-to-income ratio with zero income.");
            }

            var monthlyOutstanding = borrower.Outstanding / 12; // Rough estimate
            return monthlyOutstanding / borrower.MonthlyIncome;
        }

        public async Task<int> GetTotalActiveMoneysAsync(Guid borrowerId)
        {
            return await _creditRepository.CountAsync(l =>
                l.BorrowerId == borrowerId &&
                (l.Status == CreditStatus.Active || l.Status == CreditStatus.Overdue));
        }

        public async Task<decimal> GetTotalOutstandingAsync(Guid borrowerId)
        {
            var credits = await _creditRepository.GetListAsync(l =>
                l.BorrowerId == borrowerId &&
                (l.Status == CreditStatus.Active || l.Status == CreditStatus.Overdue));

            return credits.Sum(l => l.OutstandingAmount);
        }
    }
}