using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class InvalidPrincipalAmountException : BusinessException
    {
        public InvalidPrincipalAmountException(decimal amount)
            : base("MoneyManagement:InvalidPrincipalAmount")
        {
            WithData("PrincipalAmount", amount);
        }
    }
}
