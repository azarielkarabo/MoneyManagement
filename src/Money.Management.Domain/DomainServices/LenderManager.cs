using Money.Management.Entities;
using Money.Management.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Money.Management.DomainServices
{
    public class LenderManager : DomainService
    {
        private readonly IRepository<Lender, Guid> _lenderRepository;
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly ICurrentUser _currentUser;

        public LenderManager(
            IRepository<Lender, Guid> lenderRepository,
            IRepository<Credit, Guid> creditRepository,
            ICurrentUser currentUser)
        {
            _lenderRepository = lenderRepository;
            _creditRepository = creditRepository;
            _currentUser = currentUser;
        }

        public async Task<Lender> CreateAsync(
            Guid userId,
            string position,
            decimal maxMoneyApprovalAmount = 0)
        {
            // Check if lender already exists for this user
            var existingLender = await _lenderRepository
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (existingLender != null)
            {
                throw new InvalidOperationException($"Lender already exists for user {userId}");
            }

            // Validate position
            if (string.IsNullOrWhiteSpace(position))
            {
                throw new InvalidOperationException("Position cannot be empty.");
            }

            // Validate max Money approval amount
            if (maxMoneyApprovalAmount < 0)
            {
                throw new InvalidOperationException("Max Money approval amount cannot be negative.");
            }

            var lender = new Lender(
                GuidGenerator.Create(),
                userId,
                position,
                maxMoneyApprovalAmount
            );

            return lender;
        }

        public async Task<Lender> GetAsync(Guid id)
        {
            var lender = await _lenderRepository.GetAsync(id);

            if (lender == null)
            {
                throw new InvalidOperationException($"Lender with ID {id} not found.");
            }

            return lender;
        }

        public async Task<Lender> GetByUserIdAsync(Guid userId)
        {
            var lender = await _lenderRepository
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (lender == null)
            {
                throw new Volo.Abp.UserFriendlyException("You need a lender account for this. Apply to become a lender from the app first.");
            }

            return lender;
        }

        public async Task<Lender> GetCurrentLenderAsync()
        {
            if (!_currentUser.IsAuthenticated)
            {
                throw new InvalidOperationException("User is not authenticated.");
            }

            var currentUserId = _currentUser.Id;
            if (!currentUserId.HasValue)
            {
                throw new InvalidOperationException("Current user ID not available.");
            }

            var lender = await GetByUserIdAsync(currentUserId.Value);
            return lender;
        }

        public async Task<Lender> UpdateAsync(
            Guid id,
            string position,
            decimal? maxMoneyApprovalAmount = null)
        {
            var lender = await GetAsync(id);

            lender.UpdateProfile(position: position);

            if (maxMoneyApprovalAmount.HasValue)
                lender.UpdateApprovalLimit(maxMoneyApprovalAmount.Value);

            await _lenderRepository.UpdateAsync(lender);
            return lender;
        }

        public async Task DeleteAsync(Guid id)
        {
            var lender = await GetAsync(id);

            // Check if lender has any credits
            var lenderCredits = await _creditRepository.CountAsync(c => c.LenderId == id);

            if (lenderCredits > 0)
            {
                throw new InvalidOperationException(
                    "Cannot delete lender with existing credits.");
            }

            await _lenderRepository.DeleteAsync(id);
        }

        public async Task<bool> CanApproveAmountAsync(Guid lenderId, decimal MoneyAmount)
        {
            var lender = await GetAsync(lenderId);

            // If no limit set (0), lender can approve any amount
            if (lender.MaxMoneyApprovalAmount == 0)
            {
                return true;
            }

            return MoneyAmount <= lender.MaxMoneyApprovalAmount;
        }

        public async Task<LenderStatisticsDto> GetStatisticsAsync(Guid lenderId)
        {
            var lender = await GetAsync(lenderId);
            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);

            var statistics = new LenderStatisticsDto
            {
                LenderId = lenderId,
                LenderName = lender.User?.Name ?? "Unknown",
                Position = lender.Position,
                TotalMoneysCreated = lender.TotalMoneysCreated,
                TotalBorrowersManaged = lender.TotalBorrowersManaged,
                ActiveCredits = credits.Count(c => c.Status == CreditStatus.Active),
                OverdueCredits = credits.Count(c => c.IsLate(DateTime.UtcNow)),
                ApprovedCredits = credits.Count(c => c.Status == CreditStatus.Approved),
                PaidOffCredits = credits.Count(c => c.Status == CreditStatus.PaidOff),
                TotalAmountApproved = credits.Sum(c => c.TotalAmount),
                TotalAmountOutstanding = credits.Sum(c => c.OutstandingAmount)
            };

            return statistics;
        }

        public async Task IncrementMoneysCreatedAsync(Guid lenderId)
        {
            var lender = await GetAsync(lenderId);
            lender.IncrementMoneysCreated();
            await _lenderRepository.UpdateAsync(lender);
        }

        public async Task IncrementBorrowersManagedAsync(Guid lenderId)
        {
            var lender = await GetAsync(lenderId);
            lender.IncrementBorrowersManaged();
            await _lenderRepository.UpdateAsync(lender);
        }

        public async Task<List<Lender>> GetAllLendersAsync()
        {
            return await _lenderRepository.GetListAsync();
        }
    }

    public class LenderStatisticsDto
    {
        public Guid LenderId { get; set; }
        public string LenderName { get; set; }
        public string Position { get; set; }
        public int TotalMoneysCreated { get; set; }
        public int TotalBorrowersManaged { get; set; }
        public int ActiveCredits { get; set; }
        public int OverdueCredits { get; set; }
        public int ApprovedCredits { get; set; }
        public int PaidOffCredits { get; set; }
        public decimal TotalAmountApproved { get; set; }
        public decimal TotalAmountOutstanding { get; set; }
    }
}