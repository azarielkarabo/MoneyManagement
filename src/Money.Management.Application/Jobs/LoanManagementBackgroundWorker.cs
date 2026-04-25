using Money.Management.DomainServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace Money.Management.Jobs
{
    public class MoneyManagementBackgroundWorker : AsyncPeriodicBackgroundWorkerBase
    {
        public MoneyManagementBackgroundWorker(
            AbpAsyncTimer timer,
            IServiceScopeFactory serviceScopeFactory)
            : base(timer, serviceScopeFactory)
        {
            // Run every hour (3600000 milliseconds)
            Timer.Period = 3600000;
        }

        protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
        {
            Logger.LogInformation("MoneyManagementBackgroundWorker is running...");

            var MoneyManager = workerContext
                .ServiceProvider
                .GetRequiredService<CreditManager>();

            try
            {
                // Check and update overdue credits
                await MoneyManager.CheckAndUpdateOverdueMoneysAsync();

                Logger.LogInformation("Successfully updated overdue credit statuses");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while updating overdue credits");
            }
        }
    }
}