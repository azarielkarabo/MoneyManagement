using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class CannotUpdatePaidOffLoanException : BusinessException
    {
        public CannotUpdatePaidOffLoanException(Guid loanId)
            : base("LoanManagement:CannotUpdatePaidOffLoan")
        {
            WithData("LoanId", loanId);
        }
    }
}
