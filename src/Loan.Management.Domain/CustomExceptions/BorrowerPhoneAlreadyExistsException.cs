using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class BorrowerPhoneAlreadyExistsException : BusinessException
    {
        public BorrowerPhoneAlreadyExistsException(string phone)
            : base("LoanManagement:BorrowerPhoneAlreadyExists")
        {
            WithData("Phone", phone);
        }
    }
}
