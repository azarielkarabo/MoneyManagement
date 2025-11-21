using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class CannotDeleteLoanWithPaymentsException : BusinessException
    {
        public CannotDeleteLoanWithPaymentsException(Guid CreditId)
            : base("LoanManagement:CannotDeleteLoanWithPayments")
        {
            WithData("CreditId", CreditId);
        }
    }
}
