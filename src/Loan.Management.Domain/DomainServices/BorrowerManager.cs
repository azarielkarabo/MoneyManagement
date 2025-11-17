using Loan.Management.CustomExceptions;
using Loan.Management.Entities;
using Loan.Management.Enums;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace Loan.Management.DomainServices
{
    public class BorrowerManager : DomainService
    {
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IRepository<Entities.Loan, Guid> _loanRepository;

        public BorrowerManager(IRepository<Borrower, Guid> borrowerRepository, IRepository<Entities.Loan, Guid> loanRepository)
        {
            _borrowerRepository = borrowerRepository;
            _loanRepository = loanRepository;
        }

        public async Task<Borrower> CreateAsync(
            Guid userId,
            string address,
            string idNumber)
        {
            // Check for duplicate ID number
            var existingBorrower = await _borrowerRepository
                .FirstOrDefaultAsync(b => b.IdNumber == idNumber);

            if (existingBorrower != null)
            {
                throw new BorrowerAlreadyExistsException(idNumber);
            }

            return new Borrower(
                GuidGenerator.Create(),
                userId,
                address,
                idNumber
            );
        }

        public async Task UpdateFinancialSummaryAsync(Guid borrowerId)
        {
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
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

            borrower.UpdateFinancialSummary(totalBorrowed, outstanding, paid);

            await _borrowerRepository.UpdateAsync(borrower);
        }
    }
}
