using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class InvalidPaymentAmountException : BusinessException
    {
        public InvalidPaymentAmountException(decimal amount)
            : base("MoneyManagement:InvalidPaymentAmount")
        {
            WithData("PaymentAmount", amount);
        }
    }
}
