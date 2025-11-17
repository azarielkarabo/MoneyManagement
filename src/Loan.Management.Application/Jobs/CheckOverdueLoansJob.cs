using Loan.Management.DomainServices;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace Loan.Management.Jobs
{
    public class CheckOverdueLoansJob : AsyncBackgroundJob<CheckOverdueLoansArgs>, ITransientDependency
    {
        private readonly LoanManager _loanManager;

        public CheckOverdueLoansJob(LoanManager loanManager)
        {
            _loanManager = loanManager;
        }

        public override async Task ExecuteAsync(CheckOverdueLoansArgs args)
        {
            await _loanManager.CheckAndUpdateOverdueLoansAsync();
        }
    }

    public class CheckOverdueLoansArgs
    {
        // Add any parameters if needed
    }
}
