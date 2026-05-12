using Money.Management.Entities;
using Money.Management.Services.Budgets;
using Money.Management.Services.Budgets.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Money.Management.Services
{
    public class BudgetAppService : ApplicationService, IBudgetAppService
    {
        private readonly IRepository<Budget, Guid> _budgetRepository;
        private readonly IRepository<BudgetItem, Guid> _budgetItemRepository;

        public BudgetAppService(
            IRepository<Budget, Guid> budgetRepository,
            IRepository<BudgetItem, Guid> budgetItemRepository)
        {
            _budgetRepository = budgetRepository;
            _budgetItemRepository = budgetItemRepository;
        }

        // ── CRUD ──────────────────────────────────────────────────────────────────

        public async Task<BudgetDto> GetBudgetAsync(Guid id)
        {
            var budget = await _budgetRepository.GetAsync(id);
            var items = await _budgetItemRepository.GetListAsync(i => i.BudgetId == id);
            return ToDtoWithItems(budget, items);
        }

        public async Task<PagedResultDto<BudgetDto>> GetBudgetsAsync(GetBudgetsInput input)
        {
            var queryable = await _budgetRepository.GetQueryableAsync();

            if (input.UserId.HasValue)
                queryable = queryable.Where(b => b.UserId == input.UserId.Value);
            if (input.Month.HasValue)
                queryable = queryable.Where(b => b.Month == input.Month.Value);
            if (input.Year.HasValue)
                queryable = queryable.Where(b => b.Year == input.Year.Value);

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var budgets = await AsyncExecuter.ToListAsync(queryable);

            var dtos = new List<BudgetDto>(budgets.Count);
            foreach (var budget in budgets)
            {
                var items = await _budgetItemRepository.GetListAsync(i => i.BudgetId == budget.Id);
                dtos.Add(ToDtoWithItems(budget, items));
            }

            return new PagedResultDto<BudgetDto>(totalCount, dtos);
        }

        public async Task<BudgetDto> CreateBudgetAsync(CreateBudgetDto input)
        {
            var existing = await _budgetRepository.FindAsync(
                b => b.UserId == CurrentUser.Id.Value
                  && b.Month == input.Month
                  && b.Year == input.Year);

            if (existing != null)
                throw new InvalidOperationException(
                    $"A budget for {input.Month}/{input.Year} already exists.");

            var budget = new Budget(
                GuidGenerator.Create(),
                CurrentUser.Id.Value,
                input.Salary,
                input.Month,
                input.Year);

            await _budgetRepository.InsertAsync(budget);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ToDtoWithItems(budget, Array.Empty<BudgetItem>());
        }

        public async Task DeleteBudgetAsync(Guid id)
        {
            await _budgetRepository.DeleteAsync(id);
        }

        // ── Items ─────────────────────────────────────────────────────────────────

        public async Task<BudgetItemDto> AddBudgetItemAsync(Guid budgetId, AddBudgetItemDto input)
        {
            var budget = await LoadBudgetWithItemsAsync(budgetId);

            // AddItem enforces the salary cap and creates the child entity
            var item = budget.AddItem(
                GuidGenerator.Create(),
                input.Name,
                input.Amount,
                input.Type);

            await _budgetRepository.UpdateAsync(budget);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<BudgetItem, BudgetItemDto>(item);
        }

        public async Task<BudgetItemDto> RecordActualSpendingAsync(
            Guid budgetId, Guid itemId, decimal actualAmount)
        {
            var budget = await LoadBudgetWithItemsAsync(budgetId);

            budget.RecordActualSpending(itemId, actualAmount);

            await _budgetRepository.UpdateAsync(budget);
            await CurrentUnitOfWork.SaveChangesAsync();

            var item = await _budgetItemRepository.GetAsync(itemId);
            return ObjectMapper.Map<BudgetItem, BudgetItemDto>(item);
        }

        // ── Reports ───────────────────────────────────────────────────────────────

        public async Task<CashFlowReportDto> GetCashFlowReportAsync(Guid budgetId)
        {
            var budget = await LoadBudgetWithItemsAsync(budgetId);

            return new CashFlowReportDto
            {
                Salary = budget.Salary,
                TotalExpenses = budget.GetTotalActualExpenses(),
                TotalSavings = budget.GetTotalActualSavings(),
                TotalInvestments = budget.GetTotalInvestments(),
                RemainingBalance = budget.GetActualRemainingBalance(),
                Month = budget.Month,
                Year = budget.Year
            };
        }

        public async Task<BudgetVarianceDto> GetBudgetVarianceAsync(Guid budgetId)
        {
            var budget = await LoadBudgetWithItemsAsync(budgetId);

            var itemVariances = budget.Items.Select(i => new BudgetItemVarianceDto
            {
                Id = i.Id,
                Name = i.Name,
                Type = i.Type,
                PlannedAmount = i.Amount,
                ActualAmount = i.ActualAmount,
                Variance = i.Variance,
                IsOverBudget = i.IsOverBudget
            }).ToList();

            return new BudgetVarianceDto
            {
                BudgetId = budget.Id,
                Month = budget.Month,
                Year = budget.Year,
                Salary = budget.Salary,
                TotalPlannedExpenses = budget.GetTotalPlannedExpenses(),
                TotalActualExpenses = budget.GetTotalActualExpenses(),
                Variance = budget.GetVariance(),
                PlannedRemainingBalance = budget.GetPlannedRemainingBalance(),
                ActualRemainingBalance = budget.GetActualRemainingBalance(),
                IsOverspending = budget.IsOverspending(),
                OverspendingAmount = budget.GetOverspendingAmount(),
                Items = itemVariances
            };
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private async Task<Budget> LoadBudgetWithItemsAsync(Guid budgetId)
        {
            var budget = await _budgetRepository.GetAsync(budgetId);
            var items = await _budgetItemRepository.GetListAsync(i => i.BudgetId == budgetId);

            foreach (var item in items)
                budget.Items.ToList(); // trigger read so Items is not empty for calculations

            // Re-attach items via the domain's backing field using EF tracking
            // (Items is IReadOnlyCollection — pass items into a Budget helper that reloads them)
            return await _budgetRepository.GetAsync(budgetId, includeDetails: true);
        }

        private static BudgetDto ToDtoWithItems(Budget budget, IEnumerable<BudgetItem> items)
        {
            var itemList = items.ToList();

            // Populate backing field for calculation methods by building a transient copy
            var tempBudget = new BudgetCalculationProxy(budget, itemList);

            return new BudgetDto
            {
                Id = budget.Id,
                UserId = budget.UserId,
                Salary = budget.Salary,
                Month = budget.Month,
                Year = budget.Year,
                TotalExpenses = tempBudget.TotalPlannedExpenses,
                TotalSavings = tempBudget.TotalPlannedSavings,
                TotalInvestments = tempBudget.TotalPlannedInvestments,
                RemainingBalance = tempBudget.PlannedRemainingBalance,
                CreationTime = budget.CreationTime,
                CreatorId = budget.CreatorId,
                LastModificationTime = budget.LastModificationTime,
                LastModifierId = budget.LastModifierId,
                IsDeleted = budget.IsDeleted,
                DeletionTime = budget.DeletionTime,
                DeleterId = budget.DeleterId,
                Items = itemList.Select(i => new BudgetItemDto
                {
                    Id = i.Id,
                    BudgetId = i.BudgetId,
                    Name = i.Name,
                    Amount = i.Amount,
                    Type = i.Type,
                    CreationTime = i.CreationTime,
                    CreatorId = i.CreatorId,
                    LastModificationTime = i.LastModificationTime,
                    LastModifierId = i.LastModifierId,
                    IsDeleted = i.IsDeleted,
                    DeletionTime = i.DeletionTime,
                    DeleterId = i.DeleterId
                }).ToList()
            };
        }

        // Lightweight struct to run Budget calculation methods with a given item list
        // without mutating the tracked entity's backing field.
        private sealed class BudgetCalculationProxy
        {
            private readonly IList<BudgetItem> _items;
            private readonly Budget _budget;

            public BudgetCalculationProxy(Budget budget, IList<BudgetItem> items)
            {
                _budget = budget;
                _items = items;
            }

            public decimal TotalPlannedExpenses =>
                _items.Where(i => i.Type == Enums.BudgetItemType.Expense).Sum(i => i.Amount);

            public decimal TotalPlannedSavings =>
                _items.Where(i => i.Type == Enums.BudgetItemType.Saving).Sum(i => i.Amount);

            public decimal TotalPlannedInvestments =>
                _items.Where(i => i.Type == Enums.BudgetItemType.Investment).Sum(i => i.Amount);

            public decimal PlannedRemainingBalance => _budget.Salary - TotalPlannedExpenses;
        }
    }
}
