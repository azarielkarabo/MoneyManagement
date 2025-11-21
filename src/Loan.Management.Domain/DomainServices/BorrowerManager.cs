using Loan.Management.CustomExceptions;
using Loan.Management.Entities;
using Loan.Management.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Loan.Management.DomainServices
{
    public class BorrowerManager : DomainService
    {
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IRepository<Credit, Guid> _creditRepository;

        public BorrowerManager(
            IRepository<Borrower, Guid> borrowerRepository,
            IRepository<Credit, Guid> creditRepository)
        {
            _borrowerRepository = borrowerRepository;
            _creditRepository = creditRepository;
        }

        public async Task<Borrower> CreateAsync(
            Guid lenderId,
            string firstName,
            string lastName,
            string idNumber,
            IdentificationType idType,
            string email,
            string phoneNumber,
            string streetAddress,
            string city,
            string province,
            string postalCode,
            string country,
            DateTime dateOfBirth,
            EmploymentStatus employmentStatus,
            decimal monthlyIncome)
        {
            // Check for duplicate ID number
            var existingBorrower = await _borrowerRepository
                .FirstOrDefaultAsync(b => b.IdNumber == idNumber);

            if (existingBorrower != null)
            {
                throw new BorrowerAlreadyExistsException(idNumber);
            }

            // Validate age (optional - adjust as per business rules)
            var age = DateTime.UtcNow.Year - dateOfBirth.Year;
            if (age < 18)
            {
                throw new InvalidOperationException("Borrower must be at least 18 years old.");
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
                streetAddress,
                city,
                province,
                postalCode,
                country,
                dateOfBirth,
                employmentStatus,
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

            var defaultedLoans = credits.Count(l => l.Status == CreditStatus.Defaulted);
            var totalLoans = credits.Count(l => l.Status != CreditStatus.Rejected);

            borrower.UpdateFinancialSummary(totalBorrowed, outstanding, paid, defaultAmount, totalLoans, defaultedLoans);

            await _borrowerRepository.UpdateAsync(borrower);
        }

        public async Task UpdateKycStatusAsync(Guid borrowerId, bool isVerified)
        {
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            borrower.UpdateKycStatus(isVerified);
            await _borrowerRepository.UpdateAsync(borrower);
        }

        public async Task SuspendBorrowerAsync(Guid borrowerId, string reason)
        {
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            if (borrower.Status == BorrowerStatus.Suspended)
            {
                throw new InvalidOperationException("Borrower is already suspended.");
            }

            borrower.Status = BorrowerStatus.Suspended;
            borrower.Notes = reason;

            await _borrowerRepository.UpdateAsync(borrower);
        }

        public async Task BlacklistBorrowerAsync(Guid borrowerId, string reason)
        {
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            if (borrower.Status == BorrowerStatus.Blacklisted)
            {
                throw new InvalidOperationException("Borrower is already blacklisted.");
            }

            borrower.Status = BorrowerStatus.Blacklisted;
            borrower.Notes = reason;

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

        public async Task<int> GetTotalActiveLoansAsync(Guid borrowerId)
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