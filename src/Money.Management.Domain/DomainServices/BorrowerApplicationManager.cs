using Money.Management.Entities;
using Money.Management.Enums;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;

namespace Money.Management.DomainServices
{
    public class BorrowerApplicationManager : DomainService
    {
        private readonly IRepository<BorrowerApplication, Guid> _applicationRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IIdentityUserRepository _identityUserRepository;

        public BorrowerApplicationManager(
            IRepository<BorrowerApplication, Guid> applicationRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            IIdentityUserRepository identityUserRepository)
        {
            _applicationRepository = applicationRepository;
            _borrowerRepository = borrowerRepository;
            _identityUserRepository = identityUserRepository;
        }

        public async Task<BorrowerApplication> CreateAsync(
            string firstName,
            string lastName,
            string idNumber,
            IdentificationType idType,
            string email,
            string phoneNumber,
            string address,
            decimal monthlyIncome,
            string? message)
        {
            var existingUser = await _identityUserRepository.FindByNormalizedEmailAsync(email.ToUpperInvariant());
            if (existingUser != null)
            {
                throw new BusinessException("EMAIL_ALREADY_EXISTS").WithData("Email", email);
            }

            var existingBorrower = await _borrowerRepository.AnyAsync(b => b.IdNumber == idNumber);
            if (existingBorrower)
            {
                throw new BusinessException("BORROWER_ALREADY_EXISTS").WithData("IdNumber", idNumber);
            }

            var pendingDuplicate = await _applicationRepository.AnyAsync(a =>
                a.Status == BorrowerApplicationStatus.Pending &&
                (a.Email == email || a.IdNumber == idNumber));
            if (pendingDuplicate)
            {
                throw new BusinessException("APPLICATION_ALREADY_PENDING").WithData("Email", email);
            }

            return new BorrowerApplication(
                GuidGenerator.Create(),
                firstName,
                lastName,
                idNumber,
                idType,
                email,
                phoneNumber,
                address,
                monthlyIncome,
                message
            );
        }

        public async Task<BorrowerApplication> GetPendingAsync(Guid id)
        {
            var application = await _applicationRepository.GetAsync(id);
            if (application.Status != BorrowerApplicationStatus.Pending)
            {
                throw new BusinessException("APPLICATION_ALREADY_REVIEWED");
            }

            return application;
        }
    }
}
