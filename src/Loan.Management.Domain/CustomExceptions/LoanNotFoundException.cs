using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    // Loan Exceptions
    public class LoanNotFoundException : BusinessException
    {
        public LoanNotFoundException(Guid loanId)
            : base("LoanManagement:LoanNotFound")
        {
            WithData("LoanId", loanId);
        }
    }
}
