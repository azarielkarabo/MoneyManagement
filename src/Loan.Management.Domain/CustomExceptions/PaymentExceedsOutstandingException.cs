using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    // Payment Exceptions
    public class PaymentExceedsOutstandingException : BusinessException
    {
        public PaymentExceedsOutstandingException(decimal paymentAmount, decimal outstanding)
            : base("LoanManagement:PaymentExceedsOutstanding")
        {
            WithData("PaymentAmount", paymentAmount);
            WithData("OutstandingAmount", outstanding);
        }
    }
}
