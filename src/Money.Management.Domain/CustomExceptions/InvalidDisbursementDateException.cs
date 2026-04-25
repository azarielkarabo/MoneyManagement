using System;
using Volo.Abp;

namespace Money.Management.CustomExceptions
{
    public class InvalidDisbursementDateException : BusinessException
    {
        public InvalidDisbursementDateException(DateTime date)
            : base("MoneyManagement:InvalidDisbursementDate")
        {
            WithData("DisbursementDate", date);
        }
    }
}
