using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.LenderApplications.Dtos
{
    public class RejectLenderApplicationDto
    {
        [Required]
        [StringLength(1000)]
        public string Reason { get; set; }
    }
}
