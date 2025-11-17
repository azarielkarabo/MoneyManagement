using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class CannotDeleteLoanWithPaymentsException : BusinessException
    {
        public CannotDeleteLoanWithPaymentsException(Guid loanId)
            : base("LoanManagement:CannotDeleteLoanWithPayments")
        {
            WithData("LoanId", loanId);
        }
    }
}
