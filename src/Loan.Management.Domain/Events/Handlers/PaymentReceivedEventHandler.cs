using Loan.Management.DomainServices;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Loan.Management.Events.Handlers
{
    public class PaymentReceivedEventHandler : ILocalEventHandler<PaymentReceivedEvent>, ITransientDependency
    {
        private readonly BorrowerManager _borrowerManager;
        private readonly CreditManager _loanManager;

        public PaymentReceivedEventHandler(
            BorrowerManager borrowerManager,
            CreditManager loanManager)
        {
            _borrowerManager = borrowerManager;
            _loanManager = loanManager;
        }

        public async Task HandleEventAsync(PaymentReceivedEvent eventData)
        {
            // Update credit status
            await _loanManager.RecordPaymentAsync(
                eventData.Payment.CreditId,
                eventData.Payment.Amount);

            // Update borrower financials
            await _borrowerManager.UpdateFinancialSummaryAsync(
                eventData.Payment.BorrowerId);
        }
    }

}
