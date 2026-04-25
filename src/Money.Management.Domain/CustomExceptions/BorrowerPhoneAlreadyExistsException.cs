using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class BorrowerPhoneAlreadyExistsException : BusinessException
    {
        public BorrowerPhoneAlreadyExistsException(string phone)
            : base("MoneyManagement:BorrowerPhoneAlreadyExists")
        {
            WithData("Phone", phone);
        }
    }
}
