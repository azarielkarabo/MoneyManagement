using Loan.Management.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Loan.Management.Events.Handlers
{
    public class LoanCreatedEventHandler : ILocalEventHandler<LoanCreatedEvent>, ITransientDependency
    {
        private readonly BorrowerManager _borrowerManager;

        public LoanCreatedEventHandler(BorrowerManager borrowerManager)
        {
            _borrowerManager = borrowerManager;
        }

        public async Task HandleEventAsync(LoanCreatedEvent eventData)
        {
            // Update borrower financials when a new loan is created
            await _borrowerManager.UpdateFinancialSummaryAsync(eventData.Loan.BorrowerId);
        }
    }
}
