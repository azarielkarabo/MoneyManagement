using Money.Management.Enums;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Money.Management.Entities
{
    /// <summary>
    /// An uploaded compliance or identity document belonging to a borrower.
    /// Managed as its own aggregate so documents can be queried, verified,
    /// and rejected independently of the borrower's lifecycle.
    /// </summary>
    public class BorrowerDocument : FullAuditedAggregateRoot<Guid>
    {
        public Guid BorrowerId { get; private set; }

        public DocumentType DocumentType { get; private set; }

        /// <summary>Storage reference (URL or blob key).</summary>
        public string FileReference { get; private set; }

        public string FileName { get; private set; }

        /// <summary>File size in bytes.</summary>
        public long FileSize { get; private set; }

        public string? MimeType { get; private set; }

        public DocumentStatus Status { get; private set; }

        public DateTime? ExpiryDate { get; private set; }

        public DateTime? VerifiedAt { get; private set; }
        public Guid? VerifiedById { get; private set; }

        public string? RejectionReason { get; private set; }

        // ── EF Core constructor ───────────────────────────────────────────────────
        protected BorrowerDocument() { }

        // ── Domain constructor ────────────────────────────────────────────────────
        public BorrowerDocument(
            Guid id,
            Guid borrowerId,
            DocumentType documentType,
            string fileReference,
            string fileName,
            long fileSize,
            string mimeType,
            DateTime? expiryDate = null
        ) : base(id)
        {
            if (borrowerId == Guid.Empty)
                throw new ArgumentException("BorrowerId cannot be empty.", nameof(borrowerId));
            if (string.IsNullOrWhiteSpace(fileReference))
                throw new ArgumentException("File reference cannot be empty.", nameof(fileReference));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be empty.", nameof(fileName));
            if (fileSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(fileSize), "File size must be greater than zero.");

            BorrowerId = borrowerId;
            DocumentType = documentType;
            FileReference = fileReference;
            FileName = fileName;
            FileSize = fileSize;
            MimeType = mimeType;
            ExpiryDate = expiryDate;
            Status = DocumentStatus.Pending;
        }

        // ── State-transition methods ──────────────────────────────────────────────

        public void MarkUnderReview()
        {
            if (Status != DocumentStatus.Pending)
                throw new InvalidOperationException("Only pending documents can be placed under review.");
            Status = DocumentStatus.UnderReview;
        }

        public void Verify(Guid verifiedById)
        {
            if (Status == DocumentStatus.Rejected)
                throw new InvalidOperationException("Cannot verify a rejected document.");
            Status = DocumentStatus.Verified;
            VerifiedAt = DateTime.UtcNow;
            VerifiedById = verifiedById;
            RejectionReason = null;
        }

        public void Reject(string reason)
        {
            if (Status == DocumentStatus.Verified)
                throw new InvalidOperationException("Cannot reject a verified document.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Rejection reason cannot be empty.", nameof(reason));
            Status = DocumentStatus.Rejected;
            RejectionReason = reason;
        }
    }
}
