using Loan.Management.Entities;

namespace Loan.Management.Events
{

    public class PaymentReceivedEvent
    {
        public Payment Payment { get; set; }

        public PaymentReceivedEvent(Payment payment)
        {
            Payment = payment;
        }
    }
}
