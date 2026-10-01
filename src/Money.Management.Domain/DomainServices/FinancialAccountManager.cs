using Money.Management.Entities;
using Money.Management.Enums;
using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;
using Volo.Abp.Users;

namespace Money.Management.DomainServices
{
    public class FinancialAccountManager : DomainService
    {
        private readonly IRepository<FinancialAccount, Guid> _accountRepository;
        private readonly IRepository<AccountTransfer, Guid> _transferRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IGuidGenerator _guidGenerator;

        public FinancialAccountManager(
            IRepository<FinancialAccount, Guid> accountRepository,
            IRepository<AccountTransfer, Guid> transferRepository,
            ICurrentUser currentUser,
            IGuidGenerator guidGenerator)
        {
            _accountRepository = accountRepository;
            _transferRepository = transferRepository;
            _currentUser = currentUser;
            _guidGenerator = guidGenerator;
        }

        public Guid GetCurrentUserId()
        {
            if (!_currentUser.IsAuthenticated || !_currentUser.Id.HasValue)
                throw new InvalidOperationException("User is not authenticated.");

            return _currentUser.Id.Value;
        }

        /// <summary>
        /// Moves money between two of the current user's own accounts and records an
        /// <see cref="AccountTransfer"/> audit row. Both accounts must belong to the
        /// current user; the caller is responsible for saving changes.
        /// </summary>
        public async Task<AccountTransfer> TransferAsync(
            Guid fromAccountId, Guid toAccountId, decimal amount, string? notes)
        {
            var userId = GetCurrentUserId();

            var fromAccount = await _accountRepository.GetAsync(fromAccountId);
            var toAccount = await _accountRepository.GetAsync(toAccountId);

            if (fromAccount.UserId != userId || toAccount.UserId != userId)
                throw new UnauthorizedAccessException("You can only transfer between your own accounts.");

            fromAccount.Withdraw(amount);
            toAccount.Deposit(amount);

            await _accountRepository.UpdateAsync(fromAccount);
            await _accountRepository.UpdateAsync(toAccount);

            var transfer = new AccountTransfer(
                _guidGenerator.Create(),
                userId,
                fromAccountId,
                toAccountId,
                amount,
                notes);

            await _transferRepository.InsertAsync(transfer);

            return transfer;
        }
    }
}
