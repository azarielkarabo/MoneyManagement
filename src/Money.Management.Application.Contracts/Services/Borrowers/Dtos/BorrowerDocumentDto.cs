using Money.Management.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.Borrowers.Dtos
{
    public class BorrowerDocumentDto : FullAuditedEntityDto<Guid>
    {
        public Guid BorrowerId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FileReference { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; }
        public DocumentStatus Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid? VerifiedById { get; set; }
        public string RejectionReason { get; set; }
    }

    public class UploadBorrowerDocumentDto
    {
        public DocumentType DocumentType { get; set; }
        public string FileReference { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string MimeType { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class RejectDocumentDto
    {
        public string Reason { get; set; }
    }
}
