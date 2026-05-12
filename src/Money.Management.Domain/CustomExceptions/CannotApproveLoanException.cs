using Money.Management.Enums;
using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class CannotApproveLoanException : BusinessException
    {
        public CannotApproveLoanException(Guid loanId, LoanStatus currentStatus)
            : base("MoneyManagement:CannotApproveLoan")
        {
            WithData("LoanId", loanId);
            WithData("CurrentStatus", currentStatus.ToString());
        }
    }
}
