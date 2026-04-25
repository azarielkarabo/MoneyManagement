using Money.Management.DomainServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Money.Management.Events.Handlers
{
    public class CreditPaymentRecordedEventHandler : ILocalEventHandler<CreditPaymentRecordedEvent>, ITransientDependency
    {
        private readonly BorrowerManager _borrowerManager;

        public CreditPaymentRecordedEventHandler(BorrowerManager borrowerManager)
        {
            _borrowerManager = borrowerManager;
        }

        public async Task HandleEventAsync(CreditPaymentRecordedEvent eventData)
        {
            // Update borrower financials when a new credit is created
            await _borrowerManager.UpdateFinancialSummaryAsync(eventData.Credit.BorrowerId);
        }
    }
}
