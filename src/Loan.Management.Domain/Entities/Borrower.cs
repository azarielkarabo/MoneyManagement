using Loan.Management.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.Identity;

namespace Loan.Management.Entities
{

    public class Borrower : FullAuditedAggregateRoot<Guid>
    {
        public Guid UserId { get; set; }
        public IdentityUser User { get; set; }
        public string Address { get; set; }
        public string IdNumber { get; set; } // 13-digit SA ID
        public BorrowerStatus Status { get; set; }

        // Financial Summary (calculated fields)
        public decimal TotalBorrowed { get; set; }
        public decimal Outstanding { get; set; }
        public decimal Paid { get; set; }

        // Navigation properties
        public ICollection<Loan> Loans { get; set; }
        public ICollection<Payment> Payments { get; set; }

        protected Borrower()
        {
            Loans = new List<Loan>();
            Payments = new List<Payment>();
        }

        public Borrower(
            Guid id,
            Guid userId,
            string address,
            string idNumber
        ) : base(id)
        {
            Address = address;
            IdNumber = idNumber;
            Status = BorrowerStatus.New;
            TotalBorrowed = 0;
            Outstanding = 0;
            Paid = 0;
            Loans = new List<Loan>();
            Payments = new List<Payment>();
        }

        public void UpdateFinancialSummary(decimal totalBorrowed, decimal outstanding, decimal paid)
        {
            TotalBorrowed = totalBorrowed;
            Outstanding = outstanding;
            Paid = paid;

            // Update status based on outstanding amount
            if (Outstanding == 0 && TotalBorrowed > 0)
            {
                Status = BorrowerStatus.PaidOff;
            }
            else if (Outstanding > 0)
            {
                Status = BorrowerStatus.Active;
            }
        }
    }
}
