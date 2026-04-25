using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class InvalidMoneyDatesException : BusinessException
    {
        public InvalidMoneyDatesException()
            : base("MoneyManagement:InvalidMoneyDates")
        {
        }
    }
}
