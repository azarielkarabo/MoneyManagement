using Money.Management.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Money.Management.Services.LoanRequests
{
    public class CreateLoanRequestDto
    {
        [Required]
        public Guid LenderId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Requested amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required, StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(20)]
        public string? IdNumber { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required, StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal MonthlyIncome { get; set; }
    }

    public class ApproveLoanRequestDto
    {
        // Each loan is captured with its own interest rate.
        [Required(ErrorMessage = "Interest rate is required")]
        [Range(0, 100, ErrorMessage = "Interest rate must be between 0 and 100")]
        public decimal? InterestRate { get; set; }

        [Required]
        public DateTime ExpectedPaybackDate { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }
    }

    public class RejectLoanRequestDto
    {
        [StringLength(1000)]
        public string? Reason { get; set; }
    }

    public class GetLoanRequestsInput : PagedResultRequestDto
    {
        public LoanRequestStatus? Status { get; set; }
    }

    public class LoanRequestDto : EntityDto<Guid>
    {
        public Guid UserId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string? ApplicantEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? IdNumber { get; set; }
        public string Address { get; set; } = string.Empty;
        public decimal MonthlyIncome { get; set; }
        public Guid LenderId { get; set; }
        public string? LenderName { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public LoanRequestStatus Status { get; set; }
        public decimal? InterestRate { get; set; }
        public DateTime? ExpectedPaybackDate { get; set; }
        public string? ReviewNote { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? CreatedCreditId { get; set; }
        public DateTime CreationTime { get; set; }
    }

    // A lender an account holder can ask for a loan.
    public class LenderOptionDto : EntityDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string? Position { get; set; }
    }
}
