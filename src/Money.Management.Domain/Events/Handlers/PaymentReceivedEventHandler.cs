using Money.Management.DomainServices;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Money.Management.Events.Handlers
{
    public class PaymentReceivedEventHandler : ILocalEventHandler<PaymentReceivedEvent>, ITransientDependency
    {
        private readonly BorrowerManager _borrowerManager;
        private readonly CreditManager _MoneyManager;

        public PaymentReceivedEventHandler(
            BorrowerManager borrowerManager,
            CreditManager MoneyManager)
        {
            _borrowerManager = borrowerManager;
            _MoneyManager = MoneyManager;
        }

        public async Task HandleEventAsync(PaymentReceivedEvent eventData)
        {
            // Update credit status
            await _MoneyManager.RecordPaymentAsync(
                eventData.Payment.CreditId,
                eventData.Payment.Amount);

            // Update borrower financials
            await _borrowerManager.UpdateFinancialSummaryAsync(
                eventData.Payment.BorrowerId);
        }
    }

}
