using Money.Management.Services.Budgets.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Budgets
{
    public interface IBudgetAppService : IApplicationService
    {
        Task<BudgetDto> GetBudgetAsync(Guid id);
        Task<PagedResultDto<BudgetDto>> GetBudgetsAsync(GetBudgetsInput input);
        Task<BudgetDto> CreateBudgetAsync(CreateBudgetDto input);
        Task<BudgetItemDto> AddBudgetItemAsync(Guid budgetId, AddBudgetItemDto input);
        Task<CashFlowReportDto> GetCashFlowReportAsync(Guid budgetId);
    }
}
