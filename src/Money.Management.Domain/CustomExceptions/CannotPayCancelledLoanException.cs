using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class CannotPayCancelledMoneyException : BusinessException
    {
        public CannotPayCancelledMoneyException(Guid CreditId)
            : base("MoneyManagement:CannotPayCancelledMoney")
        {
            WithData("CreditId", CreditId);
        }
    }
}
