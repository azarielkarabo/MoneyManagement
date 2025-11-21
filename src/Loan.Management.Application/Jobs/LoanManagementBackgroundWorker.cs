using Loan.Management.DomainServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace Loan.Management.Jobs
{
    public class LoanManagementBackgroundWorker : AsyncPeriodicBackgroundWorkerBase
    {
        public LoanManagementBackgroundWorker(
            AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory)
            : base(timer, serviceScopeFactory)
        {
            // Run every hour (3600000 milliseconds)
            Timer.Period = 3600000;
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            Logger.LogInformation("LoanManagementBackgroundWorker is running...");

            var loanManager = workerContext
                .ServiceProvider
                .GetRequiredService<CreditManager>();

            try
            {
                // Check and update overdue credits
                await loanManager.CheckAndUpdateOverdueLoansAsync();

                Logger.LogInformation("Successfully updated overdue credit statuses");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while updating overdue credits");
            }
        }
    }
}