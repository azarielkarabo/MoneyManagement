using Loan.Management.DomainServices;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace Loan.Management.Events.Handlers
{
    public class PaymentReceivedEventHandler : ILocalEventHandler<PaymentReceivedEvent>, ITransientDependency
    {
        private readonly BorrowerManager _borrowerManager;
        private readonly LoanManager _loanManager;

        public PaymentReceivedEventHandler(
            BorrowerManager borrowerManager,
            LoanManager loanManager)
        {
            _borrowerManager = borrowerManager;
            _loanManager = loanManager;
        }

        public async Task HandleEventAsync(PaymentReceivedEvent eventData)
        {
            // Update loan status
            await _loanManager.RecordPaymentAsync(
                eventData.Payment.LoanId,
                eventData.Payment.Amount);

            // Update borrower financials
            await _borrowerManager.UpdateFinancialSummaryAsync(
                eventData.Payment.BorrowerId);
        }
    }

}
