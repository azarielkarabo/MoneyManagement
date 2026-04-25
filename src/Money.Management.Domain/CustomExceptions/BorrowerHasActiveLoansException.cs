using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class BorrowerHasActiveMoneysException : BusinessException
    {
        public BorrowerHasActiveMoneysException(Guid borrowerId)
            : base("MoneyManagement:BorrowerHasActiveMoneys")
        {
            WithData("BorrowerId", borrowerId);
        }
    }
}
