using Money.Management.Services.Budgets.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Budgets
{
    public interface IBudgetInsightsAppService : IApplicationService
    {
        /// <summary>
        /// Generates an AI-written summary of the current user's spending for the given month
        /// (planned vs. actual, over-budget items, savings/investment progress).
        /// Throws <see cref="Volo.Abp.UserFriendlyException"/> if no AI provider is configured.
        /// </summary>
        Task<BudgetInsightsDto> GetMonthlySummaryAsync(int year, int month);
    }
}
