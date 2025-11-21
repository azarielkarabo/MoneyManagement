using Loan.Management.Services.Borrowers.Dtos;
using Loan.Management.Services.credits.Dtos;
using Loan.Management.Services.Payments.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Loan.Management.Services.Borrowers
{
    public interface IBorrowerAppService : IApplicationService
    {
        Task<BorrowerDto> GetAsync(Guid id);

        Task<PagedResultDto<BorrowerDto>> GetListAsync(GetBorrowersInput input);

        Task<BorrowerDto> CreateAsync(CreateBorrowerDto input);

        Task<BorrowerDto> UpdateAsync(Guid id, UpdateBorrowerDto input);

        Task DeleteAsync(Guid id);

        Task<BorrowerDto> GetWithDetailsAsync(Guid id);

        Task<PagedResultDto<CreditDto>> GetBorrowerLoansAsync(Guid borrowerId, GetCreditsInput input);

        Task<PagedResultDto<PaymentDto>> GetBorrowerPaymentsAsync(Guid borrowerId, GetPaymentsInput input);

        Task RecalculateFinancialsAsync(Guid id);

        Task<bool> CheckIdNumberAvailabilityAsync(string idNumber);
    }
}
