using Money.Management.CustomExceptions;
using Money.Management.DomainServices;
using Money.Management.Entities;
using Money.Management.Enums;
using Money.Management.Permissions;
using Money.Management.Services.Borrowers;
using Money.Management.Services.Borrowers.Dtos;
using Money.Management.Services.credits.Dtos;
using Money.Management.Services.Payments.Dtos;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Money.Management.Services
{
    public class BorrowerAppService : ApplicationService, IBorrowerAppService
    {
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly IRepository<Payment, Guid> _paymentRepository;
        private readonly BorrowerManager _borrowerManager;
        private readonly LenderManager _lenderManager;
        private readonly ICurrentUser _currentUser;

        public BorrowerAppService(
            IRepository<Borrower, Guid> borrowerRepository,
            IRepository<Credit, Guid> creditRepository,
            IRepository<Payment, Guid> paymentRepository,
            BorrowerManager borrowerManager,
            LenderManager lenderManager,
            ICurrentUser currentUser)
        {
            _borrowerRepository = borrowerRepository;
            _creditRepository = creditRepository;
            _paymentRepository = paymentRepository;
            _borrowerManager = borrowerManager;
            _lenderManager = lenderManager;
            _currentUser = currentUser;
        }

        private async Task<Guid> GetCurrentLenderIdAsync()
        {
            var lender = await _lenderManager.GetCurrentLenderAsync();
            return lender.Id;
        }

        public async Task<BorrowerDto> GetAsync(Guid id)
        {
            var borrower = await _borrowerRepository.GetAsync(id);
            var dto = ObjectMapper.Map<Borrower, BorrowerDto>(borrower);

            // Add counts
            dto.ActiveMoneysCount = await _creditRepository.CountAsync(
                l => l.BorrowerId == id && l.Status == CreditStatus.Active);
            dto.TotalMoneysCount = await _creditRepository.CountAsync(l => l.BorrowerId == id);

            return dto;
        }

        public async Task<PagedResultDto<BorrowerDto>> GetListAsync(GetBorrowersInput input)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var queryable = await _borrowerRepository.GetQueryableAsync();

            // Filter by current lender
            queryable = queryable.Where(b => b.LenderId == lenderId);

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(b =>
                    b.FullName.Contains(input.Filter) ||
                    b.PhoneNumber.Contains(input.Filter) ||
                    b.EmailAddress.Contains(input.Filter) ||
                    b.IdNumber.Contains(input.Filter));
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

            // Apply sorting and paging
            queryable = queryable
                .OrderByDescending(b => b.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var borrowers = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Borrower>, List<BorrowerDto>>(borrowers);

            return new PagedResultDto<BorrowerDto>(totalCount, dtos);
        }

        public async Task<BorrowerDto> CreateAsync(CreateBorrowerDto input)
        {
            var lenderId = await GetCurrentLenderIdAsync();

            // Check for duplicate ID number within this lender
            var existingBorrower = await _borrowerRepository
                .FirstOrDefaultAsync(b => b.LenderId == lenderId && b.IdNumber == input.IdNumber);

            if (existingBorrower != null)
            {
                throw new BorrowerAlreadyExistsException(input.IdNumber);
            }

            // Create borrower using domain service
            var borrower = await _borrowerManager.CreateAsync(
                lenderId,
                input.FirstName,
                input.LastName,
                input.IdNumber,
                input.IdType,
                input.EmailAddress,
                input.PhoneNumber,
                input.StreetAddress,
                input.City,
                input.Province,
                input.PostalCode,
                input.Country,
                input.DateOfBirth,
                input.EmploymentStatus,
                input.MonthlyIncome
            );

            await _borrowerRepository.InsertAsync(borrower);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Increment borrowers managed for the lender
            await _lenderManager.IncrementBorrowersManagedAsync(lenderId);

            return await GetAsync(borrower.Id);
        }

        public async Task<BorrowerDto> UpdateAsync(Guid id, UpdateBorrowerDto input)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(id);

            // Verify borrower belongs to current lender
            if (borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this borrower.");
            }

            // Check if ID number changed and is unique within this lender
            if (borrower.IdNumber != input.IdNumber)
            {
                var existingWithIdNumber = await _borrowerRepository
                    .FirstOrDefaultAsync(b => b.LenderId == lenderId && b.IdNumber == input.IdNumber);

                if (existingWithIdNumber != null)
                {
                    throw new BorrowerAlreadyExistsException(input.IdNumber);
                }
            }

            borrower.FirstName = input.FirstName;
            borrower.LastName = input.LastName;
            borrower.FullName = $"{input.FirstName} {input.LastName}";
            borrower.IdNumber = input.IdNumber;
            borrower.IdType = input.IdType;
            borrower.EmailAddress = input.EmailAddress;
            borrower.PhoneNumber = input.PhoneNumber;
            borrower.AlternativePhoneNumber = input.AlternativePhoneNumber;
            borrower.StreetAddress = input.StreetAddress;
            borrower.City = input.City;
            borrower.Province = input.Province;
            borrower.PostalCode = input.PostalCode;
            borrower.Country = input.Country;
            borrower.DateOfBirth = input.DateOfBirth;
            borrower.EmploymentStatus = input.EmploymentStatus;
            borrower.Employer = input.Employer;
            borrower.JobTitle = input.JobTitle;
            borrower.MonthlyIncome = input.MonthlyIncome;
            borrower.EmploymentStartDate = input.EmploymentStartDate;
            borrower.MaritalStatus = input.MaritalStatus;
            borrower.NumberOfDependents = input.NumberOfDependents;
            borrower.EmergencyContactName = input.EmergencyContactName;
            borrower.EmergencyContactPhone = input.EmergencyContactPhone;
            borrower.EmergencyContactRelationship = input.EmergencyContactRelationship;
            borrower.Notes = input.Notes;

            await _borrowerRepository.UpdateAsync(borrower);

            return await GetAsync(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(id);

            // Verify borrower belongs to current lender
            if (borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this borrower.");
            }

            // Check if borrower has active credits
            var hasActiveMoneys = await _creditRepository.AnyAsync(
                l => l.BorrowerId == id &&
                (l.Status == CreditStatus.Active || l.Status == CreditStatus.Overdue));

            if (hasActiveMoneys)
            {
                throw new BorrowerHasActiveMoneysException(id);
            }

            await _borrowerRepository.DeleteAsync(id);
        }

        public async Task<BorrowerDto> GetWithDetailsAsync(Guid id)
        {
            return await GetAsync(id);
        }

        public async Task<PagedResultDto<CreditDto>> GetBorrowerMoneysAsync(
            Guid borrowerId,
            GetCreditsInput input)
        {
            var lenderId = await GetCurrentLenderIdAsync();

            // Verify borrower exists and belongs to current lender
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this borrower's Moneys.");
            }

            var queryable = await _creditRepository.GetQueryableAsync();
            queryable = queryable.Where(l => l.BorrowerId == borrowerId && l.LenderId == lenderId);

            // Apply additional filters
            if (input.Status.HasValue)
            {
                queryable = queryable.Where(l => l.Status == input.Status.Value);
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .OrderByDescending(l => l.IssuedDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var credits = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Credit>, List<CreditDto>>(credits);

            // Calculate payment progress
            foreach (var dto in dtos)
            {
                dto.PaymentProgress = dto.TotalAmount > 0
                    ? (dto.AmountPaid / dto.TotalAmount) * 100
                    : 0;
            }

            return new PagedResultDto<CreditDto>(totalCount, dtos);
        }

        public async Task<PagedResultDto<PaymentDto>> GetBorrowerPaymentsAsync(
            Guid borrowerId,
            GetPaymentsInput input)
        {
            var lenderId = await GetCurrentLenderIdAsync();

            // Verify borrower exists and belongs to current lender
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this borrower's payments.");
            }

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

        public async Task RecalculateFinancialsAsync(Guid id)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(id);

            // Verify borrower belongs to current lender
            if (borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this borrower.");
            }

            await _borrowerManager.UpdateFinancialSummaryAsync(id);
        }

        public async Task<bool> CheckIdNumberAvailabilityAsync(string idNumber)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var exists = await _borrowerRepository.AnyAsync(b => b.LenderId == lenderId && b.IdNumber == idNumber);
            return !exists;
        }

        public async Task VerifyKycAsync(Guid borrowerId)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            if (borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this borrower.");
            }

            await _borrowerManager.UpdateKycStatusAsync(borrowerId, true);
        }

        public async Task SuspendBorrowerAsync(Guid borrowerId, string reason)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            if (borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to suspend this borrower.");
            }

            await _borrowerManager.SuspendBorrowerAsync(borrowerId, reason);
        }

        public async Task BlacklistBorrowerAsync(Guid borrowerId, string reason)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            if (borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to blacklist this borrower.");
            }

            await _borrowerManager.BlacklistBorrowerAsync(borrowerId, reason);
        }

        public async Task<decimal> GetDebtToIncomeRatioAsync(Guid borrowerId)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            if (borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this borrower's information.");
            }

            return await _borrowerManager.GetDebtToIncomeRatioAsync(borrowerId);
        }
    }
}