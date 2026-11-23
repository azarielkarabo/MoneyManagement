using Money.Management.DomainServices;
using Money.Management.Entities;
using Money.Management.Enums;
using Money.Management.Permissions;
using Money.Management.Services.LenderApplications.Dtos;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Money.Management.Services.LenderApplications
{
    public class LenderApplicationAppService : ApplicationService, ILenderApplicationAppService
    {
        private readonly IRepository<LenderApplication, Guid> _applicationRepository;
        private readonly IRepository<Lender, Guid> _lenderRepository;
        private readonly LenderApplicationManager _applicationManager;
        private readonly LenderManager _lenderManager;
        private readonly CreditIdentityUserManager _userManager;
        private readonly IdentityUserManager _identityUserManager;

        public const string LenderRoleName = "lender";

        public LenderApplicationAppService(
            IRepository<LenderApplication, Guid> applicationRepository,
            IRepository<Lender, Guid> lenderRepository,
            LenderApplicationManager applicationManager,
            LenderManager lenderManager,
            CreditIdentityUserManager userManager,
            IdentityUserManager identityUserManager)
        {
            _applicationRepository = applicationRepository;
            _lenderRepository = lenderRepository;
            _applicationManager = applicationManager;
            _lenderManager = lenderManager;
            _userManager = userManager;
            _identityUserManager = identityUserManager;
        }

        [AllowAnonymous]
        public async Task<LenderApplicationDto> CreateAsync(CreateLenderApplicationDto input)
        {
            var application = await _applicationManager.CreateAsync(
                input.FirstName,
                input.LastName,
                input.Email,
                input.PhoneNumber,
                input.DesiredUserName,
                input.Position,
                input.Message
            );

            await _applicationRepository.InsertAsync(application);

            return ObjectMapper.Map<LenderApplication, LenderApplicationDto>(application);
        }

        // A signed-in account holder applies to become a lender, using their own account.
        [Authorize]
        public async Task<LenderApplicationDto> ApplyAsync(ApplyAsLenderDto input)
        {
            var user = await _identityUserManager.GetByIdAsync(CurrentUser.GetId());

            if (await _identityUserManager.IsInRoleAsync(user, LenderRoleName))
            {
                throw new UserFriendlyException("You are already a lender.");
            }
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new UserFriendlyException("Add an email address to your profile before applying.");
            }

            var application = await _applicationManager.CreateAsync(
                string.IsNullOrWhiteSpace(user.Name) ? user.UserName : user.Name,
                string.IsNullOrWhiteSpace(user.Surname) ? "-" : user.Surname,
                user.Email,
                input.PhoneNumber ?? user.PhoneNumber,
                user.UserName,
                input.Position,
                input.Message,
                forExistingUser: true
            );

            await _applicationRepository.InsertAsync(application);

            return ObjectMapper.Map<LenderApplication, LenderApplicationDto>(application);
        }

        // The signed-in user's most recent lender application, if any.
        [Authorize]
        public async Task<LenderApplicationDto?> GetMyApplicationAsync()
        {
            var email = CurrentUser.Email;
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var queryable = await _applicationRepository.GetQueryableAsync();
            var application = await AsyncExecuter.FirstOrDefaultAsync(
                queryable.Where(a => a.Email == email).OrderByDescending(a => a.CreationTime));

            return application == null ? null : ObjectMapper.Map<LenderApplication, LenderApplicationDto>(application);
        }

        [Authorize(MoneyManagementPermissions.LenderApplications.Default)]
        public async Task<LenderApplicationDto> GetAsync(Guid id)
        {
            var application = await _applicationRepository.GetAsync(id);
            return ObjectMapper.Map<LenderApplication, LenderApplicationDto>(application);
        }

        [Authorize(MoneyManagementPermissions.LenderApplications.Default)]
        public async Task<PagedResultDto<LenderApplicationDto>> GetListAsync(GetLenderApplicationsInput input)
        {
            var queryable = await _applicationRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(a =>
                    a.FirstName.Contains(input.Filter) ||
                    a.LastName.Contains(input.Filter) ||
                    a.Email.Contains(input.Filter) ||
                    a.DesiredUserName.Contains(input.Filter));
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
            var dtos = ObjectMapper.Map<System.Collections.Generic.List<LenderApplication>, System.Collections.Generic.List<LenderApplicationDto>>(applications);

            return new PagedResultDto<LenderApplicationDto>(totalCount, dtos);
        }

        [Authorize(MoneyManagementPermissions.LenderApplications.Approve)]
        public async Task<ApproveLenderApplicationResultDto> ApproveAsync(Guid id, ApproveLenderApplicationDto input)
        {
            var application = await _applicationManager.GetPendingAsync(id);

            // Applied from inside the app: the account already exists, so it just gains the
            // lender role. Otherwise (older public applications) a new account is created.
            string? temporaryPassword = null;
            var user = await _identityUserManager.FindByEmailAsync(application.Email);
            if (user == null)
            {
                temporaryPassword = GenerateTemporaryPassword();
                user = await _userManager.CreateIdentityUserAsync(
                    application.DesiredUserName,
                    application.Email,
                    temporaryPassword,
                    application.PhoneNumber,
                    application.FirstName,
                    application.LastName,
                    isActive: true,
                    tenantId: null
                );
            }
            else if (await _identityUserManager.IsInRoleAsync(user, LenderRoleName))
            {
                throw new UserFriendlyException("This person is already a lender.");
            }

            var roleResult = await _identityUserManager.AddToRoleAsync(user, LenderRoleName);
            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                throw new BusinessException("LENDER_ROLE_ASSIGNMENT_FAILED").WithData("Errors", errors);
            }

            var lender = await _lenderManager.CreateAsync(user.Id, application.Position, input.MaxMoneyApprovalAmount);
            await _lenderRepository.InsertAsync(lender);

            application.Approve(CurrentUser.Id!.Value, lender.Id);
            await _applicationRepository.UpdateAsync(application);

            await CurrentUnitOfWork!.SaveChangesAsync();

            return new ApproveLenderApplicationResultDto
            {
                LenderId = lender.Id,
                UserId = user.Id,
                UserName = user.UserName,
                TemporaryPassword = temporaryPassword,
            };
        }

        [Authorize(MoneyManagementPermissions.LenderApplications.Reject)]
        public async Task RejectAsync(Guid id, RejectLenderApplicationDto input)
        {
            var application = await _applicationManager.GetPendingAsync(id);

            application.Reject(CurrentUser.Id!.Value, input.Reason);
            await _applicationRepository.UpdateAsync(application);
        }

        private static string GenerateTemporaryPassword()
        {
            var raw = Guid.NewGuid().ToString("N").Substring(0, 12);
            return $"Lw{raw}!1";
        }
    }
}
