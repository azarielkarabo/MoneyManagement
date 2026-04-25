using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class CannotUpdatePaidOffMoneyException : BusinessException
    {
        public CannotUpdatePaidOffMoneyException(Guid CreditId)
            : base("MoneyManagement:CannotUpdatePaidOffMoney")
        {
            WithData("CreditId", CreditId);
        }
    }
}
