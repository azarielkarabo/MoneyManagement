using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.LenderApplications.Dtos
{
    public class ApproveLenderApplicationDto
    {
        [Range(0, double.MaxValue)]
        public decimal MaxMoneyApprovalAmount { get; set; } = 0;
    }
}
