using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class InvalidPaymentAmountException : BusinessException
    {
        public InvalidPaymentAmountException(decimal amount)
            : base("LoanManagement:InvalidPaymentAmount")
        {
            WithData("PaymentAmount", amount);
        }
    }
}
