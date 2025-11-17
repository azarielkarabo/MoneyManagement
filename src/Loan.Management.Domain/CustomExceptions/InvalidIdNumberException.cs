using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class InvalidIdNumberException : BusinessException
    {
        public InvalidIdNumberException(string idNumber)
            : base("LoanManagement:InvalidIdNumber")
        {
            WithData("IdNumber", idNumber);
        }
    }
}
