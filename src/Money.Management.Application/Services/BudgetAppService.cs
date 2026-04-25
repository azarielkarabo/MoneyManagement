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

        public async Task<BudgetDto> GetBudgetAsync(Guid id)
        {
            var budget = await _budgetRepository.GetAsync(id);
            var items = await _budgetItemRepository.GetListAsync(i => i.BudgetId == id);
            return MapToBudgetDto(budget, items);
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

            var dtos = new List<BudgetDto>();
            foreach (var budget in budgets)
            {
                var items = await _budgetItemRepository.GetListAsync(i => i.BudgetId == budget.Id);
                dtos.Add(MapToBudgetDto(budget, items));
            }

            return new PagedResultDto<BudgetDto>(totalCount, dtos);
        }

        public async Task<BudgetDto> CreateBudgetAsync(CreateBudgetDto input)
        {
            var existing = await _budgetRepository.FindAsync(
                b => b.UserId == CurrentUser.Id.Value &&
                     b.Month == input.Month &&
                     b.Year == input.Year);

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

            return MapToBudgetDto(budget, new List<BudgetItem>());
        }

        public async Task<BudgetItemDto> AddBudgetItemAsync(Guid budgetId, AddBudgetItemDto input)
        {
            var budget = await _budgetRepository.GetAsync(budgetId);

            var existingItems = await _budgetItemRepository.GetListAsync(i => i.BudgetId == budgetId);
            var currentTotal = existingItems.Sum(i => i.Amount);

            if (currentTotal + input.Amount > budget.Salary)
                throw new InvalidOperationException(
                    $"Adding this item would exceed the budget salary of {budget.Salary:C}. " +
                    $"Current allocated: {currentTotal:C}, New item: {input.Amount:C}.");

            var item = new BudgetItem(
                GuidGenerator.Create(),
                budgetId,
                input.Name,
                input.Amount,
                input.Type);

            await _budgetItemRepository.InsertAsync(item);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<BudgetItem, BudgetItemDto>(item);
        }

        public async Task<CashFlowReportDto> GetCashFlowReportAsync(Guid budgetId)
        {
            var budget = await _budgetRepository.GetAsync(budgetId);
            var items = await _budgetItemRepository.GetListAsync(i => i.BudgetId == budgetId);

            // Temporarily attach items for calculation methods
            foreach (var item in items)
                budget.Items.Add(item);

            return new CashFlowReportDto
            {
                Salary = budget.Salary,
                TotalExpenses = budget.GetTotalExpenses(),
                TotalSavings = budget.GetTotalSavings(),
                TotalInvestments = budget.GetTotalInvestments(),
                RemainingBalance = budget.GetRemainingBalance(),
                Month = budget.Month,
                Year = budget.Year
            };
        }

        private static BudgetDto MapToBudgetDto(Budget budget, IList<BudgetItem> items)
        {
            foreach (var item in items)
                if (!budget.Items.Contains(item))
                    budget.Items.Add(item);

            return new BudgetDto
            {
                Id = budget.Id,
                UserId = budget.UserId,
                Salary = budget.Salary,
                Month = budget.Month,
                Year = budget.Year,
                TotalExpenses = budget.GetTotalExpenses(),
                TotalSavings = budget.GetTotalSavings(),
                TotalInvestments = budget.GetTotalInvestments(),
                RemainingBalance = budget.GetRemainingBalance(),
                CreationTime = budget.CreationTime,
                CreatorId = budget.CreatorId,
                LastModificationTime = budget.LastModificationTime,
                LastModifierId = budget.LastModifierId,
                IsDeleted = budget.IsDeleted,
                DeletionTime = budget.DeletionTime,
                DeleterId = budget.DeleterId,
                Items = items.Select(i => new BudgetItemDto
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
    }
}
