using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Money.Management.DomainServices;
using Money.Management.Entities;
using Money.Management.Services.Registration;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Money.Management.Services
{
    // Public sign-up: creates an account holder who can log in straight away.
    public class RegistrationAppService : ApplicationService, IRegistrationAppService
    {
        private readonly CreditIdentityUserManager _userCreator;
        private readonly IdentityUserManager _identityUserManager;
        private readonly IIdentityUserRepository _identityUserRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;

        public RegistrationAppService(
            CreditIdentityUserManager userCreator,
            IdentityUserManager identityUserManager,
            IIdentityUserRepository identityUserRepository,
            IRepository<Borrower, Guid> borrowerRepository)
        {
            _userCreator = userCreator;
            _identityUserManager = identityUserManager;
            _identityUserRepository = identityUserRepository;
            _borrowerRepository = borrowerRepository;
        }

        [AllowAnonymous]
        public async Task RegisterAsync(RegisterAccountHolderDto input)
        {
            var userName = input.UserName.Trim();
            var email = input.Email.Trim();

            if (await _identityUserRepository.FindByNormalizedUserNameAsync(_identityUserManager.NormalizeName(userName)) != null)
            {
                throw new UserFriendlyException("That username is already taken. Please choose another one.");
            }
            if (await _identityUserRepository.FindByNormalizedEmailAsync(_identityUserManager.NormalizeEmail(email)) != null)
            {
                throw new UserFriendlyException("An account with this email already exists. Try signing in instead.");
            }

            IdentityUser user;
            try
            {
                user = await _userCreator.CreateIdentityUserAsync(
                    userName, email, input.Password, input.PhoneNumber, input.FirstName.Trim(), input.LastName.Trim());
            }
            catch (BusinessException ex) when (ex.Code == "USER_CREATION_FAILED")
            {
                // Usually the password policy (length, digits, upper/lower case, symbols).
                throw new UserFriendlyException($"Could not create your account: {ex.Data["Errors"]}");
            }

            // The account-holder role is ABP's default role and is added when the user is created
            // (CreditIdentityUserManager), once the DbMigrator has seeded it.

            // A lender may already have captured this person as a borrower; link those records
            // so the new account sees its loans and payments.
            var normalizedEmail = email.ToLowerInvariant();
            var unlinkedBorrowers = await _borrowerRepository.GetListAsync(b =>
                b.UserId == null && b.EmailAddress != null && b.EmailAddress.ToLower() == normalizedEmail);
            foreach (var borrower in unlinkedBorrowers)
            {
                borrower.AssignUser(user.Id);
            }
            if (unlinkedBorrowers.Any())
            {
                await _borrowerRepository.UpdateManyAsync(unlinkedBorrowers);
            }
        }
    }
}
