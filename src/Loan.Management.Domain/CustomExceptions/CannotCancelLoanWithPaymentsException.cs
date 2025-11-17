using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class CannotCancelLoanWithPaymentsException : BusinessException
    {
        public CannotCancelLoanWithPaymentsException(Guid loanId)
            : base("LoanManagement:CannotCancelLoanWithPayments")
        {
            WithData("LoanId", loanId);
        }
    }
}
