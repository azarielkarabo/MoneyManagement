using Money.Management.Services.Budgets.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Budgets
{
    public interface IBudgetAppService : IApplicationService
    {
        // ── CRUD ──────────────────────────────────────────────────────────────────
        Task<BudgetDto> GetBudgetAsync(Guid id);
        Task<PagedResultDto<BudgetDto>> GetBudgetsAsync(GetBudgetsInput input);
        Task<BudgetDto> CreateBudgetAsync(CreateBudgetDto input);
        Task DeleteBudgetAsync(Guid id);

        // ── Items ─────────────────────────────────────────────────────────────────
        Task<BudgetItemDto> AddBudgetItemAsync(Guid budgetId, AddBudgetItemDto input);
        Task<BudgetItemDto> AddExpenseEntryAsync(Guid budgetId, Guid itemId, AddExpenseEntryDto input);
        Task<System.Collections.Generic.List<ExpenseEntryDto>> GetExpenseEntriesAsync(Guid itemId);

        // ── Calendar ──────────────────────────────────────────────────────────────
        Task<System.Collections.Generic.List<DailySpendingDto>> GetDailySpendingAsync(int year, int month);

        // ── Reports ───────────────────────────────────────────────────────────────
        Task<CashFlowReportDto> GetCashFlowReportAsync(Guid budgetId);
        Task<BudgetVarianceDto> GetBudgetVarianceAsync(Guid budgetId);
    }
}
