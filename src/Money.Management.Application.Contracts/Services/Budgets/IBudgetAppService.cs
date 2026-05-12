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
        Task<BudgetItemDto> RecordActualSpendingAsync(Guid budgetId, Guid itemId, decimal actualAmount);

        // ── Reports ───────────────────────────────────────────────────────────────
        Task<CashFlowReportDto> GetCashFlowReportAsync(Guid budgetId);
        Task<BudgetVarianceDto> GetBudgetVarianceAsync(Guid budgetId);
    }
}
