using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    // Borrower Exceptions
    public class BorrowerAlreadyExistsException : BusinessException
    {
        public BorrowerAlreadyExistsException(string idNumber)
            : base("MoneyManagement:BorrowerAlreadyExists")
        {
            WithData("IdNumber", idNumber);
        }
    }
}
