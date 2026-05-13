// Money.Management/Services/Lenders/LenderAppService.cs
using Money.Management.DomainServices;
using Money.Management.Entities;
using Money.Management.Services.Lenders.Dtos;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace Money.Management.Services.Lenders
{
    public class LenderAppService : ApplicationService, ILenderAppService
    {
        private readonly IRepository<Lender, Guid> _lenderRepository;
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly LenderManager _lenderManager;
        private readonly CreditIdentityUserManager _userManager;

        public LenderAppService(
            IRepository<Lender, Guid> lenderRepository,
            IRepository<Credit, Guid> creditRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            LenderManager lenderManager,
            CreditIdentityUserManager userManager)
        {
            _lenderRepository = lenderRepository;
            _creditRepository = creditRepository;
            _borrowerRepository = borrowerRepository;
            _lenderManager = lenderManager;
            _userManager = userManager;
        }

        public async Task<LenderDto> GetAsync(Guid id)
        {
            var lender = await _lenderRepository.GetAsync(id);
            var dto = ObjectMapper.Map<Lender, LenderDto>(lender);

            return dto;
        }

        public async Task<LenderDto> GetCurrentLenderAsync()
        {
            var lender = await _lenderManager.GetCurrentLenderAsync();
            return await GetAsync(lender.Id);
        }

        public async Task<PagedResultDto<LenderDto>> GetListAsync(GetLendersInput input)
        {
            var queryable = await _lenderRepository.GetQueryableAsync();

            // Apply filtering
            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                queryable = queryable.Where(l =>
                    l.Position.Contains(input.Filter) ||
                    (l.User != null && l.User.Name.Contains(input.Filter)) ||
                    (l.User != null && l.User.Email.Contains(input.Filter)));
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            // Apply sorting and paging
            queryable = queryable
                .OrderByDescending(l => l.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var lenders = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<List<Lender>, List<LenderDto>>(lenders);

            return new PagedResultDto<LenderDto>(totalCount, dtos);
        }

        public async Task<LenderDto> CreateAsync(CreateLenderDto input)
        {
            var userCreate = input.UserCreate;
            var user = await _userManager.CreateIdentityUserAsync(
                userCreate.UserName,
                userCreate.Email,
                userCreate.Password,
                userCreate.PhoneNumber,
                userCreate.Name,
                userCreate.Surname,
                isActive: true,
                null);
            // Create lender using domain service
            var lender = await _lenderManager.CreateAsync(
                user.Id,
                input.Position,
                input.MaxMoneyApprovalAmount
            );

            await _lenderRepository.InsertAsync(lender);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetAsync(lender.Id);
        }

        public async Task<LenderDto> UpdateAsync(Guid id, UpdateLenderDto input)
        {
            var lender = await _lenderRepository.GetAsync(id);

            // Validate position
            if (string.IsNullOrWhiteSpace(input.Position))
            {
                throw new InvalidOperationException("Position cannot be empty.");
            }

            // Update using domain service
            await _lenderManager.UpdateAsync(
                id,
                input.Position,
                input.MaxMoneyApprovalAmount
            );

            lender = await _lenderRepository.GetAsync(id);
            return await GetAsync(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            // Use domain service to delete (includes validation)
            await _lenderManager.DeleteAsync(id);
        }

        public async Task<LenderStatisticsDto> GetStatisticsAsync(Guid id)
        {
            var statistics = await _lenderManager.GetStatisticsAsync(id);
            return statistics;
        }

        public async Task<LenderStatisticsDto> GetCurrentLenderStatisticsAsync()
        {
            var lender = await _lenderManager.GetCurrentLenderAsync();
            return await GetStatisticsAsync(lender.Id);
        }

        public async Task<LenderDto> UpdateProfileAsync(Guid id, UpdateLenderProfileDto input)
        {
            var lender = await _lenderRepository.GetAsync(id);

            lender.UpdateProfile(
                position: input.Position,
                department: input.Department,
                branch: input.Branch,
                employeeId: input.EmployeeId,
                directPhone: input.DirectPhone,
                officeLocation: input.OfficeLocation,
                bio: input.Bio);

            if (input.MaxMoneyApprovalAmount.HasValue)
                lender.UpdateApprovalLimit(input.MaxMoneyApprovalAmount.Value);

            await _lenderRepository.UpdateAsync(lender);
            return await GetAsync(id);
        }

        public async Task<LenderDto> SetAvatarAsync(Guid id, string avatarUrl)
        {
            var lender = await _lenderRepository.GetAsync(id);
            lender.SetAvatar(avatarUrl);
            await _lenderRepository.UpdateAsync(lender);
            return await GetAsync(id);
        }

        public async Task<LenderDto> GetByUserIdAsync(Guid userId)
        {
            var lender = await _lenderManager.GetByUserIdAsync(userId);
            return await GetAsync(lender.Id);
        }

        public async Task<int> GetActiveLendersCountAsync()
        {
            return await _lenderRepository.CountAsync();
        }

        public async Task<decimal> GetTotalMoneysAmountAsync(Guid lenderId)
        {
            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);
            return credits.Sum(c => c.TotalAmount);
        }

        public async Task<decimal> GetTotalOutstandingAsync(Guid lenderId)
        {
            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);
            return credits.Sum(c => c.OutstandingAmount);
        }

        public async Task<int> GetTotalBorrowersAsync(Guid lenderId)
        {
            return await _borrowerRepository.CountAsync(b => b.LenderId == lenderId);
        }

        public async Task<int> GetTotalCreditsAsync(Guid lenderId)
        {
            return await _creditRepository.CountAsync(c => c.LenderId == lenderId);
        }
    }
}