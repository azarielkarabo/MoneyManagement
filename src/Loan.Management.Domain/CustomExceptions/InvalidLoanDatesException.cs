using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class InvalidLoanDatesException : BusinessException
    {
        public InvalidLoanDatesException()
            : base("LoanManagement:InvalidLoanDates")
        {
        }
    }
}
