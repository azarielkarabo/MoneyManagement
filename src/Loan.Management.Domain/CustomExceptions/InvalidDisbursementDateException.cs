using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class InvalidDisbursementDateException : BusinessException
    {
        public InvalidDisbursementDateException(DateTime date)
            : base("LoanManagement:InvalidDisbursementDate")
        {
            WithData("DisbursementDate", date);
        }
    }
}
