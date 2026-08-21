using System;
using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.BorrowerApplications.Dtos
{
    public class ApproveBorrowerApplicationDto
    {
        /// <summary>
        /// Which lender this borrower will belong to. Any lender or admin may approve a
        /// borrower application (shared pool), so this must be specified explicitly rather
        /// than always defaulting to the caller's own lender profile (an approving admin has
        /// none).
        /// </summary>
        [Required]
        public Guid LenderId { get; set; }
    }
}
