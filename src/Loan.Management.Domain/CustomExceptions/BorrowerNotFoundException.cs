using System;
using Volo.Abp;

namespace Loan.Management.CustomExceptions
{
    public class BorrowerNotFoundException : BusinessException
    {
        public BorrowerNotFoundException(Guid borrowerId)
            : base("LoanManagement:BorrowerNotFound")
        {
            WithData("BorrowerId", borrowerId);
        }
    }
}
