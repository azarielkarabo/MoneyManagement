using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    // Payment Exceptions
    public class PaymentExceedsOutstandingException : BusinessException
    {
        public PaymentExceedsOutstandingException(decimal paymentAmount, decimal outstanding)
            : base("MoneyManagement:PaymentExceedsOutstanding")
        {
            WithData("PaymentAmount", paymentAmount);
            WithData("OutstandingAmount", outstanding);
        }
    }
}
