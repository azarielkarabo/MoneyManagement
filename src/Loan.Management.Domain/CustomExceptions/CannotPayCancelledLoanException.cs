using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class CannotPayCancelledLoanException : BusinessException
    {
        public CannotPayCancelledLoanException(Guid loanId)
            : base("LoanManagement:CannotPayCancelledLoan")
        {
            WithData("LoanId", loanId);
        }
    }
}
