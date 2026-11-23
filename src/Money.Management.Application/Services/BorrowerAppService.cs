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


        // Admins see everything (null = no lender filter); lenders only see their own data.
        private async Task<Guid?> GetLenderScopeAsync()
        {
            if (CurrentUser.IsInRole("admin"))
            {
                return null;
            }
            return (await _lenderManager.GetCurrentLenderAsync()).Id;
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Default)]
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

        [Authorize(MoneyManagementPermissions.Borrowers.Default)]
        public async Task<PagedResultDto<BorrowerDto>> GetListAsync(GetBorrowersInput input)
        {
            var lenderId = await GetLenderScopeAsync();
            var queryable = await _borrowerRepository.GetQueryableAsync();

            // Filter by current lender
            queryable = queryable.Where(b => (lenderId == null || b.LenderId == lenderId));

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

        [Authorize(MoneyManagementPermissions.Borrowers.Create)]
        public async Task<BorrowerDto> CreateAsync(CreateBorrowerDto input)
        {
            var lenderId = await GetCurrentLenderIdAsync(); // only a lender can capture a borrower

            // Check for duplicate ID number within this lender (only when one was provided)
            if (!string.IsNullOrWhiteSpace(input.IdNumber))
            {
                var existingBorrower = await _borrowerRepository
                    .FirstOrDefaultAsync(b => (lenderId == null || b.LenderId == lenderId) && b.IdNumber == input.IdNumber);

                if (existingBorrower != null)
                {
                    throw new BorrowerAlreadyExistsException(input.IdNumber);
                }
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
                input.Address,
                input.MonthlyIncome
            );

            if (input.Notes != null)
                borrower.UpdateNotes(input.Notes);

            await _borrowerRepository.InsertAsync(borrower);
            await CurrentUnitOfWork.SaveChangesAsync();

            // Increment borrowers managed for the lender
            await _lenderManager.IncrementBorrowersManagedAsync(lenderId);

            return await GetAsync(borrower.Id);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task<BorrowerDto> UpdateAsync(Guid id, UpdateBorrowerDto input)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(id);

            // Verify borrower belongs to current lender
            if (lenderId != null && borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this borrower.");
            }

            // Check if ID number changed and is unique within this lender (only when one was provided)
            if (!string.IsNullOrWhiteSpace(input.IdNumber) && borrower.IdNumber != input.IdNumber)
            {
                var existingWithIdNumber = await _borrowerRepository
                    .FirstOrDefaultAsync(b => (lenderId == null || b.LenderId == lenderId) && b.IdNumber == input.IdNumber);

                if (existingWithIdNumber != null)
                {
                    throw new BorrowerAlreadyExistsException(input.IdNumber);
                }
            }

            borrower.UpdatePersonalInfo(
                input.FirstName,
                input.LastName,
                input.IdNumber,
                input.IdType);

            borrower.UpdateContactInfo(
                input.EmailAddress,
                input.PhoneNumber);

            borrower.UpdateAddress(input.Address);

            borrower.UpdateMonthlyIncome(input.MonthlyIncome);

            if (input.Notes != null)
                borrower.UpdateNotes(input.Notes);

            await _borrowerRepository.UpdateAsync(borrower);

            return await GetAsync(id);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(id);

            // Verify borrower belongs to current lender
            if (lenderId != null && borrower.LenderId != lenderId)
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

        [Authorize(MoneyManagementPermissions.Borrowers.Default)]
        public async Task<BorrowerDto> GetWithDetailsAsync(Guid id)
        {
            return await GetAsync(id);
        }

        // ── Self-service (borrower's own portal) ──────────────────────────────────
        // These do not require Borrowers.* permissions — they resolve and act only on the
        // calling user's own Borrower record (via BorrowerManager.GetCurrentBorrowerAsync),
        // so there's nothing to gate beyond being authenticated at all.

        [Authorize]
        public async Task<BorrowerDto> GetMyProfileAsync()
        {
            var borrower = await _borrowerManager.GetCurrentBorrowerAsync();
            return await GetAsync(borrower.Id);
        }

        [Authorize]
        public async Task<BorrowerDto> UpdateMyProfileAsync(UpdateBorrowerDto input)
        {
            var borrower = await _borrowerManager.GetCurrentBorrowerAsync();

            borrower.UpdatePersonalInfo(input.FirstName, input.LastName, input.IdNumber, input.IdType);
            borrower.UpdateContactInfo(input.EmailAddress, input.PhoneNumber);
            borrower.UpdateAddress(input.Address);
            borrower.UpdateMonthlyIncome(input.MonthlyIncome);
            if (input.Notes != null)
                borrower.UpdateNotes(input.Notes);

            await _borrowerRepository.UpdateAsync(borrower);

            return await GetAsync(borrower.Id);
        }

        [Authorize]
        public async Task<PagedResultDto<CreditDto>> GetMyMoneysAsync(GetBorrowerCreditsInput input)
        {
            // An account holder may borrow from several lenders, or from none yet.
            var borrowerIds = (await _borrowerManager.GetCurrentUserBorrowersAsync()).Select(b => b.Id).ToList();

            var queryable = await _creditRepository.GetQueryableAsync();
            queryable = queryable.Where(l => borrowerIds.Contains(l.BorrowerId));

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

            var lenderNames = await GetLenderNamesAsync(credits.Select(c => c.LenderId));
            foreach (var dto in dtos)
            {
                dto.PaymentProgress = dto.TotalAmount > 0
                    ? (dto.AmountPaid / dto.TotalAmount) * 100
                    : 0;
                dto.LenderName = lenderNames.GetValueOrDefault(dto.LenderId);
            }

            return new PagedResultDto<CreditDto>(totalCount, dtos);
        }

        // Display names of lenders (their account's name and surname).
        private async Task<Dictionary<Guid, string>> GetLenderNamesAsync(IEnumerable<Guid> lenderIds)
        {
            var ids = lenderIds.Distinct().ToList();
            var lenderRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<Lender, Guid>>();
            var userRepository = LazyServiceProvider.LazyGetRequiredService<IRepository<Volo.Abp.Identity.IdentityUser, Guid>>();

            var lenders = await lenderRepository.GetListAsync(l => ids.Contains(l.Id));
            var userIds = lenders.Select(l => l.UserId).ToList();
            var users = (await userRepository.GetListAsync(u => userIds.Contains(u.Id))).ToDictionary(u => u.Id);

            return lenders.ToDictionary(l => l.Id, l =>
            {
                if (!users.TryGetValue(l.UserId, out var u)) return "Lender";
                var full = string.Join(" ", new[] { u.Name, u.Surname }.Where(x => !string.IsNullOrWhiteSpace(x)));
                return string.IsNullOrWhiteSpace(full) ? u.UserName : full;
            });
        }

        [Authorize]
        public async Task<PagedResultDto<PaymentDto>> GetMyPaymentsAsync(GetBorrowerPaymentsInput input)
        {
            var borrowerIds = (await _borrowerManager.GetCurrentUserBorrowersAsync()).Select(b => b.Id).ToList();

            var queryable = await _paymentRepository.GetQueryableAsync();
            queryable = queryable.Where(p => borrowerIds.Contains(p.BorrowerId));

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .OrderByDescending(p => p.PaymentDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var payments = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Payment>, List<PaymentDto>>(payments);

            return new PagedResultDto<PaymentDto>(totalCount, dtos);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.ViewFinancials)]
        public async Task<PagedResultDto<CreditDto>> GetBorrowerMoneysAsync(
            Guid borrowerId,
            GetBorrowerCreditsInput input)
        {
            var lenderId = await GetLenderScopeAsync();

            // Verify borrower exists and belongs to current lender
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (lenderId != null && borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this borrower's Moneys.");
            }

            var queryable = await _creditRepository.GetQueryableAsync();
            queryable = queryable.Where(l => l.BorrowerId == borrowerId && (lenderId == null || l.LenderId == lenderId));

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

        [Authorize(MoneyManagementPermissions.Borrowers.ViewFinancials)]
        public async Task<PagedResultDto<PaymentDto>> GetBorrowerPaymentsAsync(
            Guid borrowerId,
            GetBorrowerPaymentsInput input)
        {
            var lenderId = await GetLenderScopeAsync();

            // Verify borrower exists and belongs to current lender
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (lenderId != null && borrower.LenderId != lenderId)
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

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task RecalculateFinancialsAsync(Guid id)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(id);

            // Verify borrower belongs to current lender
            if (lenderId != null && borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this borrower.");
            }

            await _borrowerManager.UpdateFinancialSummaryAsync(id);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Default)]
        public async Task<bool> CheckIdNumberAvailabilityAsync(string idNumber)
        {
            var lenderId = await GetLenderScopeAsync();
            var exists = await _borrowerRepository.AnyAsync(b => (lenderId == null || b.LenderId == lenderId) && b.IdNumber == idNumber);
            return !exists;
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task SuspendBorrowerAsync(Guid borrowerId, string reason)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            if (lenderId != null && borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to suspend this borrower.");
            }

            await _borrowerManager.SuspendBorrowerAsync(borrowerId, reason);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task BlacklistBorrowerAsync(Guid borrowerId, string reason)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            if (lenderId != null && borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to blacklist this borrower.");
            }

            await _borrowerManager.BlacklistBorrowerAsync(borrowerId, reason);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.ViewFinancials)]
        public async Task<decimal> GetDebtToIncomeRatioAsync(Guid borrowerId)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);

            if (lenderId != null && borrower.LenderId != lenderId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view this borrower's information.");
            }

            return await _borrowerManager.GetDebtToIncomeRatioAsync(borrowerId);
        }

        // ── Document management ───────────────────────────────────────────────────

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task<BorrowerDocumentDto> UploadDocumentAsync(Guid borrowerId, UploadBorrowerDocumentDto input)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (lenderId != null && borrower.LenderId != lenderId)
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

        [Authorize(MoneyManagementPermissions.Borrowers.Default)]
        public async Task<PagedResultDto<BorrowerDocumentDto>> GetDocumentsAsync(
            Guid borrowerId,
            PagedAndSortedResultRequestDto input)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (lenderId != null && borrower.LenderId != lenderId)
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

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task<BorrowerDocumentDto> VerifyDocumentAsync(Guid borrowerId, Guid documentId)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (lenderId != null && borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to verify this document.");

            var document = await _documentRepository.GetAsync(documentId);
            if (document.BorrowerId != borrowerId)
                throw new InvalidOperationException("Document does not belong to this borrower.");

            var verifierId = _currentUser.Id ?? throw new InvalidOperationException("Current user ID not available.");
            document.Verify(verifierId);

            await _documentRepository.UpdateAsync(document);
            return ObjectMapper.Map<BorrowerDocument, BorrowerDocumentDto>(document);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task<BorrowerDocumentDto> RejectDocumentAsync(
            Guid borrowerId,
            Guid documentId,
            RejectDocumentDto input)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (lenderId != null && borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to reject this document.");

            var document = await _documentRepository.GetAsync(documentId);
            if (document.BorrowerId != borrowerId)
                throw new InvalidOperationException("Document does not belong to this borrower.");

            document.Reject(input.Reason);
            await _documentRepository.UpdateAsync(document);
            return ObjectMapper.Map<BorrowerDocument, BorrowerDocumentDto>(document);
        }

        // ── Bank account management ───────────────────────────────────────────────

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task<BorrowerBankAccountDto> AddBankAccountAsync(
            Guid borrowerId,
            AddBorrowerBankAccountDto input)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId, includeDetails: true);
            if (lenderId != null && borrower.LenderId != lenderId)
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

        [Authorize(MoneyManagementPermissions.Borrowers.Default)]
        public async Task<IReadOnlyList<BorrowerBankAccountDto>> GetBankAccountsAsync(Guid borrowerId)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId, includeDetails: true);
            if (lenderId != null && borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to view this borrower's bank accounts.");

            return ObjectMapper.Map<IReadOnlyCollection<BorrowerBankAccount>, List<BorrowerBankAccountDto>>(
                borrower.BankAccounts);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task SetPrimaryBankAccountAsync(Guid borrowerId, Guid accountId)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId, includeDetails: true);
            if (lenderId != null && borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to manage this borrower's bank accounts.");

            borrower.SetPrimaryBankAccount(accountId);
            await _borrowerRepository.UpdateAsync(borrower);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task VerifyBankAccountAsync(Guid borrowerId, Guid accountId)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId, includeDetails: true);
            if (lenderId != null && borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to manage this borrower's bank accounts.");

            var account = borrower.BankAccounts.FirstOrDefault(a => a.Id == accountId)
                ?? throw new InvalidOperationException($"Bank account {accountId} not found.");

            account.Verify();
            await _borrowerRepository.UpdateAsync(borrower);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task DeleteBankAccountAsync(Guid borrowerId, Guid accountId)
        {
            var lenderId = await GetLenderScopeAsync();
            var borrower = await _borrowerRepository.GetAsync(borrowerId);
            if (lenderId != null && borrower.LenderId != lenderId)
                throw new UnauthorizedAccessException("You do not have permission to manage this borrower's bank accounts.");

            var account = await _bankAccountRepository.GetAsync(accountId);
            if (account.BorrowerId != borrowerId)
                throw new InvalidOperationException("Bank account does not belong to this borrower.");

            await _bankAccountRepository.DeleteAsync(accountId);
        }
    }
}