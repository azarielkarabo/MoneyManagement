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
        private readonly IRepository<BorrowerDocument, Guid> _documentRepository;
        private readonly IRepository<BorrowerBankAccount, Guid> _bankAccountRepository;
        private readonly BorrowerManager _borrowerManager;
        private readonly LenderManager _lenderManager;
        private readonly ICurrentUser _currentUser;

        public BorrowerAppService(
            IRepository<Borrower, Guid> borrowerRepository,
            IRepository<Credit, Guid> creditRepository,
            IRepository<Payment, Guid> paymentRepository,
            IRepository<BorrowerDocument, Guid> documentRepository,
            IRepository<BorrowerBankAccount, Guid> bankAccountRepository,
            BorrowerManager borrowerManager,
            LenderManager lenderManager,
            ICurrentUser currentUser)
        {
            _borrowerRepository = borrowerRepository;
            _creditRepository = creditRepository;
            _paymentRepository = paymentRepository;
            _documentRepository = documentRepository;
            _bankAccountRepository = bankAccountRepository;
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

            borrower.UpdatePersonalInfo(
                input.FirstName,
                input.LastName,
                input.DateOfBirth,
                input.MaritalStatus,
                input.NumberOfDependents,
                input.IdNumber,
                input.IdType,
                input.IdExpiryDate);

            borrower.UpdateContactInfo(
                input.EmailAddress,
                input.PhoneNumber,
                input.AlternativePhoneNumber);

            borrower.UpdateAddress(
                input.StreetAddress,
                input.City,
                input.Province,
                input.PostalCode,
                input.Country);

            borrower.UpdateEmploymentInfo(
                input.EmploymentStatus,
                input.Employer,
                input.JobTitle,
                input.MonthlyIncome,
                input.EmploymentStartDate);

            borrower.UpdateEmergencyContact(
                input.EmergencyContactName,
                input.EmergencyContactPhone,
                input.EmergencyContactRelationship);

            if (input.Notes != null)
                borrower.UpdateNotes(input.Notes);

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

        // ── Profile management ────────────────────────────────────────────────────

        public async Task<BorrowerDto> SetAvatarAsync(Guid id, string avatarUrl)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(id);
            if (borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to update this borrower.");

            borrower.SetAvatar(avatarUrl);
            await _borrowerRepository.UpdateAsync(borrower);
            return await GetAsync(id);
        }

        public async Task<BorrowerDto> UpdateCreditScoreAsync(Guid id, int creditScore)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(id);
            if (borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to update this borrower.");

            borrower.UpdateCreditScore(creditScore);
            await _borrowerRepository.UpdateAsync(borrower);
            return await GetAsync(id);
        }

        // ── Document management ───────────────────────────────────────────────────

        public async Task<BorrowerDocumentDto> UploadDocumentAsync(Guid borrowerId, UploadBorrowerDocumentDto input)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to manage this borrower's documents.");

            var document = new BorrowerDocument(
                GuidGenerator.Create(),
                borrowerId,
                input.DocumentType,
                input.FileReference,
                input.FileName,
                input.FileSize,
                input.MimeType,
                input.ExpiryDate);

            await _documentRepository.InsertAsync(document);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<BorrowerDocument, BorrowerDocumentDto>(document);
        }

        public async Task<PagedResultDto<BorrowerDocumentDto>> GetDocumentsAsync(
            Guid borrowerId,
            PagedAndSortedResultRequestDto input)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to view this borrower's documents.");

            var queryable = await _documentRepository.GetQueryableAsync();
            queryable = queryable.Where(d => d.BorrowerId == borrowerId);

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .OrderByDescending(d => d.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var docs = await AsyncExecuter.ToListAsync(queryable);
            return new PagedResultDto<BorrowerDocumentDto>(
                totalCount,
                ObjectMapper.Map<List<BorrowerDocument>, List<BorrowerDocumentDto>>(docs));
        }

        public async Task<BorrowerDocumentDto> VerifyDocumentAsync(Guid borrowerId, Guid documentId)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to verify this document.");

            var document = await _documentRepository.GetAsync(documentId);
            if (document.BorrowerId != borrowerId)
                throw new InvalidOperationException("Document does not belong to this borrower.");

            var verifierId = _currentUser.Id ?? throw new InvalidOperationException("Current user ID not available.");
            document.Verify(verifierId);

            await _documentRepository.UpdateAsync(document);
            return ObjectMapper.Map<BorrowerDocument, BorrowerDocumentDto>(document);
        }

        public async Task<BorrowerDocumentDto> RejectDocumentAsync(
            Guid borrowerId,
            Guid documentId,
            RejectDocumentDto input)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to reject this document.");

            var document = await _documentRepository.GetAsync(documentId);
            if (document.BorrowerId != borrowerId)
                throw new InvalidOperationException("Document does not belong to this borrower.");

            document.Reject(input.Reason);
            await _documentRepository.UpdateAsync(document);
            return ObjectMapper.Map<BorrowerDocument, BorrowerDocumentDto>(document);
        }

        // ── Bank account management ───────────────────────────────────────────────

        public async Task<BorrowerBankAccountDto> AddBankAccountAsync(
            Guid borrowerId,
            AddBorrowerBankAccountDto input)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId, includeDetails: true);
            if (borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to manage this borrower's bank accounts.");

            var account = borrower.AddBankAccount(
                GuidGenerator.Create(),
                input.BankName,
                input.BranchCode,
                input.AccountNumber,
                input.AccountType,
                input.AccountHolderName,
                input.IsPrimary);

            await _borrowerRepository.UpdateAsync(borrower);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<BorrowerBankAccount, BorrowerBankAccountDto>(account);
        }

        public async Task<IReadOnlyList<BorrowerBankAccountDto>> GetBankAccountsAsync(Guid borrowerId)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId, includeDetails: true);
            if (borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to view this borrower's bank accounts.");

            return ObjectMapper.Map<IReadOnlyCollection<BorrowerBankAccount>, List<BorrowerBankAccountDto>>(
                borrower.BankAccounts);
        }

        public async Task SetPrimaryBankAccountAsync(Guid borrowerId, Guid accountId)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId, includeDetails: true);
            if (borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to manage this borrower's bank accounts.");

            borrower.SetPrimaryBankAccount(accountId);
            await _borrowerRepository.UpdateAsync(borrower);
        }

        public async Task VerifyBankAccountAsync(Guid borrowerId, Guid accountId)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId, includeDetails: true);
            if (borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to manage this borrower's bank accounts.");

            var account = borrower.BankAccounts.FirstOrDefault(a => a.Id == accountId)
                ?? throw new InvalidOperationException($"Bank account {accountId} not found.");

            account.Verify();
            await _borrowerRepository.UpdateAsync(borrower);
        }

        public async Task DeleteBankAccountAsync(Guid borrowerId, Guid accountId)
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to manage this borrower's bank accounts.");

            var account = await _bankAccountRepository.GetAsync(accountId);
            if (account.BorrowerId != borrowerId)
                throw new InvalidOperationException("Bank account does not belong to this borrower.");

            await _bankAccountRepository.DeleteAsync(accountId);
        }
    }
}