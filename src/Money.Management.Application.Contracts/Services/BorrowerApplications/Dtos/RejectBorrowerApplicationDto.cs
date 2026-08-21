using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.BorrowerApplications.Dtos
{
    public class RejectBorrowerApplicationDto
    {
        [Required]
        [StringLength(1000)]
        public string Reason { get; set; }
    }
}
