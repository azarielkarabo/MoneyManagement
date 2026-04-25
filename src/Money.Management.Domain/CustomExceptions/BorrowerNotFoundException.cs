using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class BorrowerNotFoundException : BusinessException
    {
        public BorrowerNotFoundException(Guid borrowerId)
            : base("MoneyManagement:BorrowerNotFound")
        {
            WithData("BorrowerId", borrowerId);
        }
    }
}
