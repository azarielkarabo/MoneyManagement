using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class BorrowerHasActiveLoansException : BusinessException
    {
        public BorrowerHasActiveLoansException(Guid borrowerId)
            : base("LoanManagement:BorrowerHasActiveLoans")
        {
            WithData("BorrowerId", borrowerId);
        }
    }
}
