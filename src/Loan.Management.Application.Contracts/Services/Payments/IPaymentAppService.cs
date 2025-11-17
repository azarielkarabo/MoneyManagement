using Loan.Management.Services.Payments.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Loan.Management.Services.Payments
{
    public interface IPaymentAppService : IApplicationService
    {
        Task<PaymentDto> GetAsync(Guid id);

        Task<PagedResultDto<PaymentDto>> GetListAsync(GetPaymentsInput input);

        Task<PaymentDto> CreateAsync(CreatePaymentDto input);

        Task DeleteAsync(Guid id);

        Task<List<PaymentsByBorrowerDto>> GetPaymentsByBorrowerAsync(GetPaymentsInput input);

        Task<PaymentDto> ApprovePaymentAsync(Guid id);

        Task<decimal> GetTotalPaymentsAsync(Guid? borrowerId = null, Guid? loanId = null);

        Task<List<PaymentDto>> GetRecentPaymentsAsync(int count = 10);
    }
}
