using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class CannotPayCancelledLoanException : BusinessException
    {
        public CannotPayCancelledLoanException(Guid CreditId)
            : base("LoanManagement:CannotPayCancelledLoan")
        {
            WithData("CreditId", CreditId);
        }
    }
}
