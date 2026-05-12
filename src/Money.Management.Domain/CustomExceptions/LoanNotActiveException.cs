using Money.Management.Enums;
using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class LoanNotActiveException : BusinessException
    {
        public LoanNotActiveException(Guid loanId, LoanStatus currentStatus)
            : base("MoneyManagement:LoanNotActive")
        {
            WithData("LoanId", loanId);
            WithData("CurrentStatus", currentStatus.ToString());
        }
    }
}
