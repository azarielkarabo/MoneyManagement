using Microsoft.AspNetCore.Authorization;
using Money.Management.DomainServices;
using Money.Management.Entities;
using Money.Management.Enums;
using Money.Management.Permissions;
using Money.Management.Services.LoanRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace Money.Management.Services
{
    // Account holders ask lenders for loans; lenders approve (setting the interest rate and
    // expected payback date, which creates the loan) or reject them.
    public class LoanRequestAppService : ApplicationService, ILoanRequestAppService
    {
        private const string AdminRoleName = "admin";

        private readonly IRepository<LoanRequest, Guid> _requestRepository;
        private readonly IRepository<Lender, Guid> _lenderRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly IRepository<IdentityUser, Guid> _userRepository;
        private readonly BorrowerManager _borrowerManager;
        private readonly LenderManager _lenderManager;

        public LoanRequestAppService(
            IRepository<LoanRequest, Guid> requestRepository,
            IRepository<Lender, Guid> lenderRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            IRepository<Credit, Guid> creditRepository,
            IRepository<IdentityUser, Guid> userRepository,
            BorrowerManager borrowerManager,
            LenderManager lenderManager)
        {
            _requestRepository = requestRepository;
            _lenderRepository = lenderRepository;
            _borrowerRepository = borrowerRepository;
            _creditRepository = creditRepository;
            _userRepository = userRepository;
            _borrowerManager = borrowerManager;
            _lenderManager = lenderManager;
        }

        // ───────────── Account holder side ─────────────

        [Authorize]
        public async Task<List<LenderOptionDto>> GetLenderOptionsAsync()
        {
            var lenders = await _lenderRepository.GetListAsync();
            var me = CurrentUser.GetId();
            lenders = lenders.Where(l => l.UserId != me).ToList();

            var names = await GetUserNamesAsync(lenders.Select(l => l.UserId));
            return lenders
                .Select(l => new LenderOptionDto
                {
                    Id = l.Id,
                    Name = names.GetValueOrDefault(l.UserId) ?? "Lender",
                    Position = l.Position,
                })
                .OrderBy(l => l.Name)
                .ToList();
        }

        [Authorize]
        public async Task<LoanRequestDto> CreateAsync(CreateLoanRequestDto input)
        {
            var me = CurrentUser.GetId();
            var lender = await _lenderRepository.FindAsync(input.LenderId)
                ?? throw new UserFriendlyException("That lender no longer exists.");
            if (lender.UserId == me)
            {
                throw new UserFriendlyException("You cannot request a loan from yourself.");
            }

            var user = await _userRepository.GetAsync(me);
            var name = string.Join(" ", new[] { user.Name, user.Surname }.Where(s => !string.IsNullOrWhiteSpace(s)));

            var request = new LoanRequest(
                GuidGenerator.Create(),
                me,
                string.IsNullOrWhiteSpace(name) ? user.UserName : name,
                user.Email,
                input.PhoneNumber ?? user.PhoneNumber,
                string.IsNullOrWhiteSpace(input.IdNumber) ? null : input.IdNumber.Trim(),
                input.Address.Trim(),
                input.MonthlyIncome,
                lender.Id,
                input.Amount,
                input.Reason.Trim());

            await _requestRepository.InsertAsync(request, autoSave: true);
            return (await MapAsync(new List<LoanRequest> { request })).Single();
        }

        [Authorize]
        public async Task<PagedResultDto<LoanRequestDto>> GetMyListAsync(GetLoanRequestsInput input)
        {
            var me = CurrentUser.GetId();
            var queryable = (await _requestRepository.GetQueryableAsync()).Where(r => r.UserId == me);
            return await PageAsync(queryable, input);
        }

        [Authorize]
        public async Task CancelAsync(Guid id)
        {
            var request = await _requestRepository.GetAsync(id);
            if (request.UserId != CurrentUser.GetId())
            {
                throw new UserFriendlyException("You can only cancel your own loan requests.");
            }
            request.Cancel();
            await _requestRepository.UpdateAsync(request);
        }

        // ───────────── Lender side ─────────────

        [Authorize(MoneyManagementPermissions.Credits.Default)]
        public async Task<PagedResultDto<LoanRequestDto>> GetIncomingListAsync(GetLoanRequestsInput input)
        {
            var queryable = await _requestRepository.GetQueryableAsync();

            // Lenders see requests sent to them; admins see all requests.
            if (!CurrentUser.IsInRole(AdminRoleName))
            {
                var lender = await _lenderManager.GetCurrentLenderAsync();
                queryable = queryable.Where(r => r.LenderId == lender.Id);
            }

            return await PageAsync(queryable, input);
        }

        [Authorize(MoneyManagementPermissions.Credits.Create)]
        public async Task<LoanRequestDto> ApproveAsync(Guid id, ApproveLoanRequestDto input)
        {
            var request = await GetReviewableAsync(id);

            var borrower = await FindOrCreateBorrowerAsync(request);

            var credit = new Credit(
                GuidGenerator.Create(),
                borrower.Id,
                request.LenderId,
                request.Amount,
                input.InterestRate!.Value,
                0, // open-ended: no fixed repayment term
                input.ExpectedPaybackDate,
                request.Reason.Length > 500 ? request.Reason[..500] : request.Reason);
            credit.Notes = $"From loan request of {Clock.Now:yyyy-MM-dd}";
            await _creditRepository.InsertAsync(credit, autoSave: true);

            request.Approve(credit.Id, input.InterestRate!.Value, input.ExpectedPaybackDate, input.Note, Clock.Now);
            await _requestRepository.UpdateAsync(request, autoSave: true);

            await _borrowerManager.UpdateFinancialSummaryAsync(borrower.Id);

            return (await MapAsync(new List<LoanRequest> { request })).Single();
        }

        [Authorize(MoneyManagementPermissions.Credits.Create)]
        public async Task<LoanRequestDto> RejectAsync(Guid id, RejectLoanRequestDto input)
        {
            var request = await GetReviewableAsync(id);
            request.Reject(input.Reason, Clock.Now);
            await _requestRepository.UpdateAsync(request);
            return (await MapAsync(new List<LoanRequest> { request })).Single();
        }

        // ───────────── Helpers ─────────────

        private async Task<LoanRequest> GetReviewableAsync(Guid id)
        {
            var request = await _requestRepository.GetAsync(id);
            if (!CurrentUser.IsInRole(AdminRoleName))
            {
                var lender = await _lenderManager.GetCurrentLenderAsync();
                if (request.LenderId != lender.Id)
                {
                    throw new UserFriendlyException("This loan request was sent to another lender.");
                }
            }
            if (request.Status != LoanRequestStatus.Pending)
            {
                throw new UserFriendlyException("This loan request has already been dealt with.");
            }
            return request;
        }

        // The applicant's borrower record with this lender: linked by account, then matched by
        // email, otherwise created from the details on the request.
        private async Task<Borrower> FindOrCreateBorrowerAsync(LoanRequest request)
        {
            var borrower = await _borrowerRepository.FirstOrDefaultAsync(b =>
                b.LenderId == request.LenderId && b.UserId == request.UserId);
            if (borrower != null)
            {
                return borrower;
            }

            if (!string.IsNullOrWhiteSpace(request.ApplicantEmail))
            {
                var email = request.ApplicantEmail.ToLower();
                borrower = await _borrowerRepository.FirstOrDefaultAsync(b =>
                    b.LenderId == request.LenderId && b.UserId == null &&
                    b.EmailAddress != null && b.EmailAddress.ToLower() == email);
                if (borrower != null)
                {
                    borrower.AssignUser(request.UserId);
                    await _borrowerRepository.UpdateAsync(borrower, autoSave: true);
                    return borrower;
                }
            }

            var nameParts = request.ApplicantName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            borrower = await _borrowerManager.CreateAsync(
                request.LenderId,
                nameParts.ElementAtOrDefault(0) ?? request.ApplicantName,
                nameParts.ElementAtOrDefault(1) ?? "-",
                request.IdNumber,
                IdentificationType.SouthAfricanId,
                request.ApplicantEmail,
                request.PhoneNumber,
                request.Address,
                request.MonthlyIncome);
            borrower.AssignUser(request.UserId);
            await _borrowerRepository.InsertAsync(borrower, autoSave: true);
            return borrower;
        }

        private async Task<PagedResultDto<LoanRequestDto>> PageAsync(IQueryable<LoanRequest> queryable, GetLoanRequestsInput input)
        {
            if (input.Status.HasValue)
            {
                queryable = queryable.Where(r => r.Status == input.Status.Value);
            }

            var totalCount = await AsyncExecuter.CountAsync(queryable);
            var items = await AsyncExecuter.ToListAsync(queryable
                .OrderByDescending(r => r.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

            return new PagedResultDto<LoanRequestDto>(totalCount, await MapAsync(items));
        }

        private async Task<List<LoanRequestDto>> MapAsync(List<LoanRequest> requests)
        {
            var lenderIds = requests.Select(r => r.LenderId).Distinct().ToList();
            var lenders = await _lenderRepository.GetListAsync(l => lenderIds.Contains(l.Id));
            var lenderNames = await GetUserNamesAsync(lenders.Select(l => l.UserId));
            var lenderNameById = lenders.ToDictionary(l => l.Id, l => lenderNames.GetValueOrDefault(l.UserId));

            return requests.Select(r => new LoanRequestDto
            {
                Id = r.Id,
                UserId = r.UserId,
                ApplicantName = r.ApplicantName,
                ApplicantEmail = r.ApplicantEmail,
                PhoneNumber = r.PhoneNumber,
                IdNumber = r.IdNumber,
                Address = r.Address,
                MonthlyIncome = r.MonthlyIncome,
                LenderId = r.LenderId,
                LenderName = lenderNameById.GetValueOrDefault(r.LenderId),
                Amount = r.Amount,
                Reason = r.Reason,
                Status = r.Status,
                InterestRate = r.InterestRate,
                ExpectedPaybackDate = r.ExpectedPaybackDate,
                ReviewNote = r.ReviewNote,
                ReviewedAt = r.ReviewedAt,
                CreatedCreditId = r.CreatedCreditId,
                CreationTime = r.CreationTime,
            }).ToList();
        }

        private async Task<Dictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> userIds)
        {
            var ids = userIds.Distinct().ToList();
            var users = await _userRepository.GetListAsync(u => ids.Contains(u.Id));
            return users.ToDictionary(
                u => u.Id,
                u =>
                {
                    var full = string.Join(" ", new[] { u.Name, u.Surname }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    return string.IsNullOrWhiteSpace(full) ? u.UserName : full;
                });
        }
    }
}
