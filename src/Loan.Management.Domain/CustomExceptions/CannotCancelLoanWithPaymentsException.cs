using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class CannotCancelLoanWithPaymentsException : BusinessException
    {
        public CannotCancelLoanWithPaymentsException(Guid CreditId)
            : base("LoanManagement:CannotCancelLoanWithPayments")
        {
            WithData("CreditId", CreditId);
        }
    }
}
