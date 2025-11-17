using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    // Borrower Exceptions
    public class BorrowerAlreadyExistsException : BusinessException
    {
        public BorrowerAlreadyExistsException(string idNumber)
            : base("LoanManagement:BorrowerAlreadyExists")
        {
            WithData("IdNumber", idNumber);
        }
    }
}
