using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class LoanAlreadyCompletedException : BusinessException
    {
        public LoanAlreadyCompletedException(Guid loanId)
            : base("MoneyManagement:LoanAlreadyCompleted")
        {
            WithData("LoanId", loanId);
        }
    }
}
