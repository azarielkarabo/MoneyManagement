using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    // Loan Exceptions
    public class LoanNotFoundException : BusinessException
    {
        public LoanNotFoundException(Guid CreditId)
            : base("LoanManagement:LoanNotFound")
        {
            WithData("CreditId", CreditId);
        }
    }
}
