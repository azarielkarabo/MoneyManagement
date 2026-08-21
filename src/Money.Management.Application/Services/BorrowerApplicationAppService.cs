using Money.Management.DomainServices;
using Money.Management.Entities;
using Money.Management.Permissions;
using Money.Management.Services.BorrowerApplications.Dtos;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Money.Management.Services.BorrowerApplications
{
    public class BorrowerApplicationAppService : ApplicationService, IBorrowerApplicationAppService
    {
        private readonly IRepository<BorrowerApplication, Guid> _applicationRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IRepository<Lender, Guid> _lenderRepository;
        private readonly BorrowerApplicationManager _applicationManager;
        private readonly BorrowerManager _borrowerManager;
        private readonly LenderManager _lenderManager;
        private readonly CreditIdentityUserManager _userManager;

        public BorrowerApplicationAppService(
            IRepository<BorrowerApplication, Guid> applicationRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            IRepository<Lender, Guid> lenderRepository,
            BorrowerApplicationManager applicationManager,
            BorrowerManager borrowerManager,
            LenderManager lenderManager,
            CreditIdentityUserManager userManager)
        {
            _applicationRepository = applicationRepository;
            _borrowerRepository = borrowerRepository;
            _lenderRepository = lenderRepository;
            _applicationManager = applicationManager;
            _borrowerManager = borrowerManager;
            _lenderManager = lenderManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<BorrowerApplicationDto> CreateAsync(CreateBorrowerApplicationDto input)
        {
            var application = await _applicationManager.CreateAsync(
                input.FirstName,
                input.LastName,
                input.IdNumber,
                input.IdType,
                input.Email,
                input.PhoneNumber,
                input.Address,
                input.MonthlyIncome,
                input.Message
            );

            await _applicationRepository.InsertAsync(application);

            return ObjectMapper.Map<BorrowerApplication, BorrowerApplicationDto>(application);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Default)]
        public async Task<BorrowerApplicationDto> GetAsync(Guid id)
        {
            var application = await _applicationRepository.GetAsync(id);
            return ObjectMapper.Map<BorrowerApplication, BorrowerApplicationDto>(application);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Default)]
        public async Task<PagedResultDto<BorrowerApplicationDto>> GetListAsync(GetBorrowerApplicationsInput input)
        {
            var queryable = await _applicationRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(a =>
                    a.FirstName.Contains(input.Filter) ||
                    a.LastName.Contains(input.Filter) ||
                    a.Email.Contains(input.Filter) ||
                    a.IdNumber.Contains(input.Filter));
            }

            if (input.Status.HasValue)
            {
                queryable = queryable.Where(a => a.Status == input.Status.Value);
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .OrderByDescending(a => a.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var applications = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<System.Collections.Generic.List<BorrowerApplication>, System.Collections.Generic.List<BorrowerApplicationDto>>(applications);

            return new PagedResultDto<BorrowerApplicationDto>(totalCount, dtos);
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Create)]
        public async Task<ApproveBorrowerApplicationResultDto> ApproveAsync(Guid id, ApproveBorrowerApplicationDto input)
        {
            var application = await _applicationManager.GetPendingAsync(id);

            var lender = await _lenderRepository.FindAsync(input.LenderId)
                ?? throw new BusinessException("LENDER_NOT_FOUND").WithData("LenderId", input.LenderId);

            var desiredUserName = await GenerateUniqueUserNameAsync(application.FirstName, application.LastName);
            var temporaryPassword = GenerateTemporaryPassword();

            var user = await _userManager.CreateIdentityUserAsync(
                desiredUserName,
                application.Email,
                temporaryPassword,
                application.PhoneNumber,
                application.FirstName,
                application.LastName,
                isActive: true,
                tenantId: null
            );

            var borrower = await _borrowerManager.CreateAsync(
                lender.Id,
                application.FirstName,
                application.LastName,
                application.IdNumber,
                application.IdType,
                application.Email,
                application.PhoneNumber,
                application.Address,
                application.MonthlyIncome
            );
            borrower.AssignUser(user.Id);

            await _borrowerRepository.InsertAsync(borrower);
            await _lenderManager.IncrementBorrowersManagedAsync(lender.Id);

            application.Approve(CurrentUser.Id!.Value, borrower.Id);
            await _applicationRepository.UpdateAsync(application);

            await CurrentUnitOfWork!.SaveChangesAsync();

            return new ApproveBorrowerApplicationResultDto
            {
                BorrowerId = borrower.Id,
                UserId = user.Id,
                UserName = user.UserName,
                TemporaryPassword = temporaryPassword,
            };
        }

        [Authorize(MoneyManagementPermissions.Borrowers.Edit)]
        public async Task RejectAsync(Guid id, RejectBorrowerApplicationDto input)
        {
            var application = await _applicationManager.GetPendingAsync(id);

            application.Reject(CurrentUser.Id!.Value, input.Reason);
            await _applicationRepository.UpdateAsync(application);
        }

        private async Task<string> GenerateUniqueUserNameAsync(string firstName, string lastName)
        {
            // Letters/numbers/hyphens/underscores only, to match the frontend login form's
            // username validation regex (which rejects dots and other punctuation).
            var baseUserName = $"{firstName}{lastName}".Replace(" ", "").ToLowerInvariant();
            var candidate = baseUserName;
            var suffix = 1;

            while (await _userManager.IsUserNameTakenAsync(candidate))
            {
                candidate = $"{baseUserName}{suffix}";
                suffix++;
            }

            return candidate;
        }

        private static string GenerateTemporaryPassword()
        {
            var raw = Guid.NewGuid().ToString("N").Substring(0, 12);
            return $"Bw{raw}!1";
        }
    }
}
