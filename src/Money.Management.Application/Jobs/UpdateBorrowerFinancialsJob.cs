using Money.Management.DomainServices;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Money.Management.Jobs
{
    public class UpdateBorrowerFinancialsJob : AsyncBackgroundJob<UpdateBorrowerFinancialsArgs>, ITransientDependency
    {
        private readonly BorrowerManager _borrowerManager;

        public UpdateBorrowerFinancialsJob(BorrowerManager borrowerManager)
        {
            _borrowerManager = borrowerManager;
        }

        public override async Task ExecuteAsync(UpdateBorrowerFinancialsArgs args)
        {
            await _borrowerManager.UpdateFinancialSummaryAsync(args.BorrowerId);
        }
    }

    public class UpdateBorrowerFinancialsArgs
    {
        public Guid BorrowerId { get; set; }
    }
}
