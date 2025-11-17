using Loan.Management.CustomExceptions;
using Loan.Management.DomainServices;
using Loan.Management.Entities;
using Loan.Management.Enums;
using Loan.Management.Permissions;
using Loan.Management.Services.Borrowers;
using Loan.Management.Services.Borrowers.Dtos;
using Loan.Management.Services.Loans.Dtos;
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

namespace Loan.Management.Services
{
    [Authorize(LoanManagementPermissions.Borrowers.Default)]
    public class BorrowerAppService : ApplicationService, IBorrowerAppService
    {
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IRepository<Entities.Loan, Guid> _loanRepository;
        private readonly IRepository<Payment, Guid> _paymentRepository;
        private readonly BorrowerManager _borrowerManager;
        private readonly LoanIdentityUserManager _loanIdentityUserManager;

        public BorrowerAppService(
            IRepository<Borrower, Guid> borrowerRepository,
            IRepository<Entities.Loan, Guid> loanRepository,
            IRepository<Payment, Guid> paymentRepository,
            BorrowerManager borrowerManager,
            LoanIdentityUserManager loanIdentityUserManager)
        {
            _borrowerRepository = borrowerRepository;
            _loanRepository = loanRepository;
            _paymentRepository = paymentRepository;
            _borrowerManager = borrowerManager;
            _loanIdentityUserManager = loanIdentityUserManager;
        }

        public async Task<BorrowerDto> GetAsync(Guid id)
        {
            var borrower = await _borrowerRepository.GetAsync(id);
            var dto = ObjectMapper.Map<Borrower, BorrowerDto>(borrower);

            // Add counts
            dto.ActiveLoansCount = await _loanRepository.CountAsync(
                l => l.BorrowerId == id && l.Status == LoanStatus.Active);
            dto.TotalLoansCount = await _loanRepository.CountAsync(l => l.BorrowerId == id);

            return dto;
        }

        public async Task<PagedResultDto<BorrowerDto>> GetListAsync(GetBorrowersInput input)
        {
            var queryable = await _borrowerRepository.GetQueryableAsync();

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(b =>
                    b.User != null && (
                    b.User.Name.Contains(input.Filter) ||
                    b.User.PhoneNumber.Contains(input.Filter) ||
                    b.User.Email.Contains(input.Filter) ||
                    b.IdNumber.Contains(input.Filter)));
            }

            if (input.Status.HasValue)
            {
                queryable = queryable.Where(b => b.Status == input.Status.Value);
            }

            if (input.MinOutstanding.HasValue)
            {
                queryable = queryable.Where(b => b.Outstanding >= input.MinOutstanding.Value);
            }

            if (input.MaxOutstanding.HasValue)
            {
                queryable = queryable.Where(b => b.Outstanding <= input.MaxOutstanding.Value);
            }

            // Get total count
            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply sorting
            //if (!string.IsNullOrWhiteSpace(input.Sorting))
            //{
            //    queryable = queryable.OrderBy(input.Sorting);
            //}
            //else
            //{
            queryable = queryable.OrderByDescending(b => b.CreationTime);
            //}

            // Apply paging
            queryable = queryable
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var borrowers = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Borrower>, List<BorrowerDto>>(borrowers);

            return new PagedResultDto<BorrowerDto>(totalCount, dtos);
        }

        [Authorize(LoanManagementPermissions.Borrowers.Create)]
        public async Task<BorrowerDto> CreateAsync(CreateBorrowerDto input)
        {
            // Use domain service to create borrower (includes business logic)
            var creatUser = input.UserCreate;

            var identityUser = await _loanIdentityUserManager.CreateIdentityUserAsync(
                creatUser.UserName,
                creatUser.Email,
                creatUser.Password,
                creatUser.PhoneNumber,
                creatUser.Name,
                creatUser.Surname,
                isActive: true,
                null);

            var borrower = await _borrowerManager.CreateAsync(
                identityUser.Id,
                input.Address,
                input.IdNumber
            );

            await _borrowerRepository.InsertAsync(borrower);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetAsync(borrower.Id);
        }

        [Authorize(LoanManagementPermissions.Borrowers.Edit)]
        public async Task<BorrowerDto> UpdateAsync(Guid id, UpdateBorrowerDto input)
        {
            var borrower = await _borrowerRepository.GetAsync(id);

            // Check if ID number changed and is unique
            if (borrower.IdNumber != input.IdNumber)
            {
                var existingWithIdNumber = await _borrowerRepository
                    .FirstOrDefaultAsync(b => b.IdNumber == input.IdNumber);

                if (existingWithIdNumber != null)
                {
                    throw new BorrowerAlreadyExistsException(input.IdNumber);
                }
            }

            var userUpdate = input.UserUpdate;

            var userUpdateResults = await _loanIdentityUserManager.UpdateAsync(borrower.UserId,
                          userUpdate.UserName,
                          userUpdate.Email,
                          userUpdate.PhoneNumber,
                          userUpdate.Name,
                          userUpdate.Surname,
                          isActive: null);

            borrower.Address = input.Address;
            borrower.IdNumber = input.IdNumber;

            await _borrowerRepository.UpdateAsync(borrower);

            return await GetAsync(id);
        }

        [Authorize(LoanManagementPermissions.Borrowers.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            // Check if borrower has active loans
            var hasActiveLoans = await _loanRepository.AnyAsync(
                l => l.BorrowerId == id &&
                (l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue));

            if (hasActiveLoans)
            {
                throw new BorrowerHasActiveLoansException(id);
            }

            await _borrowerRepository.DeleteAsync(id);
        }

        [Authorize(LoanManagementPermissions.Borrowers.ViewFinancials)]
        public async Task<BorrowerDto> GetWithDetailsAsync(Guid id)
        {
            return await GetAsync(id);
        }

        public async Task<PagedResultDto<LoanDto>> GetBorrowerLoansAsync(
            Guid borrowerId,
            GetLoansInput input)
        {
            input.BorrowerId = borrowerId;

            var queryable = await _loanRepository.GetQueryableAsync();
            queryable = queryable.Where(l => l.BorrowerId == borrowerId);

            // Apply additional filters
            if (input.Status.HasValue)
            {
                queryable = queryable.Where(l => l.Status == input.Status.Value);
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .OrderByDescending(l => l.DisbursementDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var loans = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Entities.Loan>, List<LoanDto>>(loans);

            // Calculate payment progress
            foreach (var dto in dtos)
            {
                dto.PaymentProgress = dto.TotalAmount > 0
                    ? (dto.AmountPaid / dto.TotalAmount) * 100
                    : 0;
            }

            return new PagedResultDto<LoanDto>(totalCount, dtos);
        }

        public async Task<PagedResultDto<PaymentDto>> GetBorrowerPaymentsAsync(
            Guid borrowerId,
            GetPaymentsInput input)
        {
            input.BorrowerId = borrowerId;

            var queryable = await _paymentRepository.GetQueryableAsync();
            queryable = queryable.Where(p => p.BorrowerId == borrowerId);

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .OrderByDescending(p => p.PaymentDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var payments = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Payment>, List<PaymentDto>>(payments);

            return new PagedResultDto<PaymentDto>(totalCount, dtos);
        }

        [Authorize(LoanManagementPermissions.Borrowers.Edit)]
        public async Task RecalculateFinancialsAsync(Guid id)
        {
            await _borrowerManager.UpdateFinancialSummaryAsync(id);
        }

        public async Task<bool> CheckIdNumberAvailabilityAsync(string idNumber)
        {
            var exists = await _borrowerRepository.AnyAsync(b => b.IdNumber == idNumber);
            return !exists;
        }
    }
}
