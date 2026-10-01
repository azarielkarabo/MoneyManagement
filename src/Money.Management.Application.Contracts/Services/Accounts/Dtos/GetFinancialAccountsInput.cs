using Money.Management.Enums;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Accounts.Dtos
{
    public class GetFinancialAccountsInput : PagedAndSortedResultRequestDto
    {
        public FinancialAccountType? AccountType { get; set; }
        public bool? IsActive { get; set; }
    }
}
