using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class InvalidIdNumberException : BusinessException
    {
        public InvalidIdNumberException(string idNumber)
            : base("MoneyManagement:InvalidIdNumber")
        {
            WithData("IdNumber", idNumber);
        }
    }
}
