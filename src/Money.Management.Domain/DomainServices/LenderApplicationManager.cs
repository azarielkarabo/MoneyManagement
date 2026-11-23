using Money.Management.Entities;
using Money.Management.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;

namespace Money.Management.DomainServices
{
    public class LenderApplicationManager : DomainService
    {
        private readonly IRepository<LenderApplication, Guid> _applicationRepository;
        private readonly IIdentityUserRepository _identityUserRepository;

        public LenderApplicationManager(
            IRepository<LenderApplication, Guid> applicationRepository,
            IIdentityUserRepository identityUserRepository)
        {
            _applicationRepository = applicationRepository;
            _identityUserRepository = identityUserRepository;
        }

        public async Task<LenderApplication> CreateAsync(
            string firstName,
            string lastName,
            string email,
            string? phoneNumber,
            string desiredUserName,
            string position,
            string? message,
            bool forExistingUser = false)
        {
            // An account holder applying from inside the app already owns this email/username.
            if (!forExistingUser)
            {
            var existingByEmail = await _identityUserRepository.FindByNormalizedEmailAsync(email.ToUpperInvariant());
            if (existingByEmail != null)
            {
                throw new BusinessException("EMAIL_ALREADY_EXISTS").WithData("Email", email);
            }

            var existingByUserName = await _identityUserRepository.FindByNormalizedUserNameAsync(desiredUserName.ToUpperInvariant());
            if (existingByUserName != null)
            {
                throw new BusinessException("USERNAME_ALREADY_EXISTS").WithData("UserName", desiredUserName);
            }
            }

            var pendingDuplicate = await _applicationRepository.AnyAsync(a =>
                a.Status == LenderApplicationStatus.Pending &&
                (a.Email == email || a.DesiredUserName == desiredUserName));
            if (pendingDuplicate)
            {
                throw new BusinessException("APPLICATION_ALREADY_PENDING")
                    .WithData("Email", email);
            }

            return new LenderApplication(
                GuidGenerator.Create(),
                firstName,
                lastName,
                email,
                phoneNumber,
                desiredUserName,
                position,
                message
            );
        }

        public async Task<LenderApplication> GetPendingAsync(Guid id)
        {
            var application = await _applicationRepository.GetAsync(id);
            if (application.Status != LenderApplicationStatus.Pending)
            {
                throw new BusinessException("APPLICATION_ALREADY_REVIEWED");
            }

            return application;
        }
    }
}
