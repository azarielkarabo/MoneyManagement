using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Budgets.Dtos
{
    public class GetBudgetsInput : PagedAndSortedResultRequestDto
    {
        public Guid? UserId { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
