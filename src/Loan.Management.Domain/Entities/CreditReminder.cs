using Loan.Management.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Loan.Management.Entities
{
    public class CreditReminder : FullAuditedEntity<Guid>
    {
        public CreditReminder(
           Guid creditId,
           ReminderType type)
        {
            CreditId = creditId;
            Type = type;
        }

        public virtual required Guid CreditId { get; set; }

        public virtual Credit Credit { get; set; }

        public virtual ReminderType Type { get; set; }
    }
}
