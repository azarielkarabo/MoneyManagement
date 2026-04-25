using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class CannotDeleteMoneyWithPaymentsException : BusinessException
    {
        public CannotDeleteMoneyWithPaymentsException(Guid CreditId)
            : base("MoneyManagement:CannotDeleteMoneyWithPayments")
        {
            WithData("CreditId", CreditId);
        }
    }
}
