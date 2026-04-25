using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class CannotCancelMoneyWithPaymentsException : BusinessException
    {
        public CannotCancelMoneyWithPaymentsException(Guid CreditId)
            : base("MoneyManagement:CannotCancelMoneyWithPayments")
        {
            WithData("CreditId", CreditId);
        }
    }
}
