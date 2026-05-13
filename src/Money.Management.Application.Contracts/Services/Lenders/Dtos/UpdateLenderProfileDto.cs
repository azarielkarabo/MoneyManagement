using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Lenders.Dtos
{
    public class UpdateLenderProfileDto
    {
        [StringLength(100)]
        public string Position { get; set; }

        [StringLength(100)]
        public string Department { get; set; }

        [StringLength(100)]
        public string Branch { get; set; }

        [StringLength(50)]
        public string EmployeeId { get; set; }

        [StringLength(20)]
        public string DirectPhone { get; set; }

        [StringLength(200)]
        public string OfficeLocation { get; set; }

        [StringLength(1000)]
        public string Bio { get; set; }

        public decimal? MaxMoneyApprovalAmount { get; set; }
    }
}
