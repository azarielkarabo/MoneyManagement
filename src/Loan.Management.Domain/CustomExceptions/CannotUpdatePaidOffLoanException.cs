using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class CannotUpdatePaidOffLoanException : BusinessException
    {
        public CannotUpdatePaidOffLoanException(Guid CreditId)
            : base("LoanManagement:CannotUpdatePaidOffLoan")
        {
            WithData("CreditId", CreditId);
        }
    }
}
