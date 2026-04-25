using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    // Money Exceptions
    public class MoneyNotFoundException : BusinessException
    {
        public MoneyNotFoundException(Guid CreditId)
            : base("MoneyManagement:MoneyNotFound")
        {
            WithData("CreditId", CreditId);
        }
    }
}
