using Money.Management.Entities;

namespace Money.Management.Events
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
