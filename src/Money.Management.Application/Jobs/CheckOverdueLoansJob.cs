using Money.Management.DomainServices;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Money.Management.Jobs
{
    public class CheckOverdueMoneysJob : AsyncBackgroundJob<CheckOverdueMoneysArgs>, ITransientDependency
    {
        private readonly CreditManager _MoneyManager;

        public CheckOverdueMoneysJob(CreditManager MoneyManager)
        {
            _MoneyManager = MoneyManager;
        }

        public override async Task ExecuteAsync(CheckOverdueMoneysArgs args)
        {
            await _MoneyManager.CheckAndUpdateOverdueMoneysAsync();
        }
    }

    public class CheckOverdueMoneysArgs
    {
        // Add any parameters if needed
    }
}
