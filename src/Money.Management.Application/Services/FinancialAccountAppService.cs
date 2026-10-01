using Money.Management.DomainServices;
using Money.Management.Entities;
using Money.Management.Permissions;
using Money.Management.Services.Accounts;
using Money.Management.Services.Accounts.Dtos;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Money.Management.Services
{
    public class FinancialAccountAppService : ApplicationService, IFinancialAccountAppService
    {
        private readonly IRepository<FinancialAccount, Guid> _accountRepository;
        private readonly IRepository<AccountTransfer, Guid> _transferRepository;
        private readonly FinancialAccountManager _accountManager;

        public FinancialAccountAppService(
            IRepository<FinancialAccount, Guid> accountRepository,
            IRepository<AccountTransfer, Guid> transferRepository,
            FinancialAccountManager accountManager)
        {
            _accountRepository = accountRepository;
            _transferRepository = transferRepository;
            _accountManager = accountManager;
        }

        private async Task<FinancialAccount> GetOwnedAccountAsync(Guid id)
        {
            var account = await _accountRepository.GetAsync(id);

            if (account.UserId != CurrentUser.Id.Value)
                throw new UnauthorizedAccessException("You do not have permission to access this account.");

            return account;
        }

        [Authorize(MoneyManagementPermissions.Accounts.Default)]
        public async Task<FinancialAccountDto> GetAsync(Guid id)
        {
            var account = await GetOwnedAccountAsync(id);
            return ObjectMapper.Map<FinancialAccount, FinancialAccountDto>(account);
        }

        [Authorize(MoneyManagementPermissions.Accounts.Default)]
        public async Task<PagedResultDto<FinancialAccountDto>> GetListAsync(GetFinancialAccountsInput input)
        {
            var queryable = await _accountRepository.GetQueryableAsync();

            queryable = queryable.Where(a => a.UserId == CurrentUser.Id.Value);

            if (input.AccountType.HasValue)
                queryable = queryable.Where(a => a.AccountType == input.AccountType.Value);

            if (input.IsActive.HasValue)
                queryable = queryable.Where(a => a.IsActive == input.IsActive.Value);

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .OrderBy(a => a.AccountType)
                .ThenBy(a => a.Name)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var accounts = await AsyncExecuter.ToListAsync(queryable);
            var dtos = ObjectMapper.Map<System.Collections.Generic.List<FinancialAccount>, System.Collections.Generic.List<FinancialAccountDto>>(accounts);

            return new PagedResultDto<FinancialAccountDto>(totalCount, dtos);
        }

        [Authorize(MoneyManagementPermissions.Accounts.Create)]
        public async Task<FinancialAccountDto> CreateAsync(CreateFinancialAccountDto input)
        {
            var account = new FinancialAccount(
                GuidGenerator.Create(),
                CurrentUser.Id.Value,
                input.Name,
                input.AccountType,
                input.OpeningBalance);

            await _accountRepository.InsertAsync(account);

            return ObjectMapper.Map<FinancialAccount, FinancialAccountDto>(account);
        }

        [Authorize(MoneyManagementPermissions.Accounts.Edit)]
        public async Task<FinancialAccountDto> UpdateAsync(Guid id, UpdateFinancialAccountDto input)
        {
            var account = await GetOwnedAccountAsync(id);
            account.Rename(input.Name);

            await _accountRepository.UpdateAsync(account);

            return ObjectMapper.Map<FinancialAccount, FinancialAccountDto>(account);
        }

        [Authorize(MoneyManagementPermissions.Accounts.Edit)]
        public async Task<FinancialAccountDto> DeactivateAsync(Guid id)
        {
            var account = await GetOwnedAccountAsync(id);
            account.Deactivate();

            await _accountRepository.UpdateAsync(account);

            return ObjectMapper.Map<FinancialAccount, FinancialAccountDto>(account);
        }

        [Authorize(MoneyManagementPermissions.Accounts.Edit)]
        public async Task<FinancialAccountDto> ActivateAsync(Guid id)
        {
            var account = await GetOwnedAccountAsync(id);
            account.Activate();

            await _accountRepository.UpdateAsync(account);

            return ObjectMapper.Map<FinancialAccount, FinancialAccountDto>(account);
        }

        [Authorize(MoneyManagementPermissions.Accounts.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            await GetOwnedAccountAsync(id);
            await _accountRepository.DeleteAsync(id);
        }

        [Authorize(MoneyManagementPermissions.Accounts.Transfer)]
        public async Task<AccountTransferDto> TransferAsync(TransferFundsDto input)
        {
            var transfer = await _accountManager.TransferAsync(
                input.FromAccountId, input.ToAccountId, input.Amount, input.Notes);

            await CurrentUnitOfWork.SaveChangesAsync();

            return await MapTransferAsync(transfer);
        }

        [Authorize(MoneyManagementPermissions.Accounts.Default)]
        public async Task<PagedResultDto<AccountTransferDto>> GetTransfersAsync(PagedAndSortedResultRequestDto input)
        {
            var queryable = await _transferRepository.GetQueryableAsync();

            queryable = queryable.Where(t => t.UserId == CurrentUser.Id.Value);

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .OrderByDescending(t => t.TransferDate)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var transfers = await AsyncExecuter.ToListAsync(queryable);

            var dtos = new System.Collections.Generic.List<AccountTransferDto>(transfers.Count);
            foreach (var transfer in transfers)
                dtos.Add(await MapTransferAsync(transfer));

            return new PagedResultDto<AccountTransferDto>(totalCount, dtos);
        }

        private async Task<AccountTransferDto> MapTransferAsync(AccountTransfer transfer)
        {
            var dto = ObjectMapper.Map<AccountTransfer, AccountTransferDto>(transfer);

            var fromAccount = await _accountRepository.GetAsync(transfer.FromAccountId);
            var toAccount = await _accountRepository.GetAsync(transfer.ToAccountId);

            dto.FromAccountName = fromAccount.Name;
            dto.ToAccountName = toAccount.Name;

            return dto;
        }
    }
}
