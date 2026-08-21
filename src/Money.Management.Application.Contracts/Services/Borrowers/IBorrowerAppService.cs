using Money.Management.Services.Borrowers.Dtos;
using Money.Management.Services.credits.Dtos;
using Money.Management.Services.Payments.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.Borrowers
{
    public interface IBorrowerAppService : IApplicationService
    {
        Task<BorrowerDto> GetAsync(Guid id);

        Task<PagedResultDto<BorrowerDto>> GetListAsync(GetBorrowersInput input);

        Task<BorrowerDto> CreateAsync(CreateBorrowerDto input);

        Task<BorrowerDto> UpdateAsync(Guid id, UpdateBorrowerDto input);

        Task DeleteAsync(Guid id);

        Task<BorrowerDto> GetWithDetailsAsync(Guid id);

        // ── Self-service (borrower's own portal) ──────────────────────────────────
        // Scoped to the calling user's own Borrower record — no Borrowers.* permission
        // required, since the caller can only ever see/edit their own data.
        Task<BorrowerDto> GetMyProfileAsync();
        Task<BorrowerDto> UpdateMyProfileAsync(UpdateBorrowerDto input);
        Task<PagedResultDto<CreditDto>> GetMyMoneysAsync(GetBorrowerCreditsInput input);
        Task<PagedResultDto<PaymentDto>> GetMyPaymentsAsync(GetBorrowerPaymentsInput input);

        Task<PagedResultDto<CreditDto>> GetBorrowerMoneysAsync(Guid borrowerId, GetBorrowerCreditsInput input);

        Task<PagedResultDto<PaymentDto>> GetBorrowerPaymentsAsync(Guid borrowerId, GetBorrowerPaymentsInput input);

        Task RecalculateFinancialsAsync(Guid id);

        Task<bool> CheckIdNumberAvailabilityAsync(string idNumber);

        // ── Document management ───────────────────────────────────────────────────
        Task<BorrowerDocumentDto> UploadDocumentAsync(Guid borrowerId, UploadBorrowerDocumentDto input);

        Task<PagedResultDto<BorrowerDocumentDto>> GetDocumentsAsync(Guid borrowerId, PagedAndSortedResultRequestDto input);

        Task<BorrowerDocumentDto> VerifyDocumentAsync(Guid borrowerId, Guid documentId);

        Task<BorrowerDocumentDto> RejectDocumentAsync(Guid borrowerId, Guid documentId, RejectDocumentDto input);

        // ── Bank account management ───────────────────────────────────────────────
        Task<BorrowerBankAccountDto> AddBankAccountAsync(Guid borrowerId, AddBorrowerBankAccountDto input);

        Task<IReadOnlyList<BorrowerBankAccountDto>> GetBankAccountsAsync(Guid borrowerId);

        Task SetPrimaryBankAccountAsync(Guid borrowerId, Guid accountId);

        Task VerifyBankAccountAsync(Guid borrowerId, Guid accountId);

        Task DeleteBankAccountAsync(Guid borrowerId, Guid accountId);
    }
}
