using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class InvalidPrincipalAmountException : BusinessException
    {
        public InvalidPrincipalAmountException(decimal amount)
            : base("LoanManagement:InvalidPrincipalAmount")
        {
            WithData("PrincipalAmount", amount);
        }
    }
}
