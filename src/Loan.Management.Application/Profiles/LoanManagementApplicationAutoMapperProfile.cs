using AutoMapper;
using Loan.Management.Entities;
using Loan.Management.Services.Borrowers.Dtos;
using Loan.Management.Services.credits.Dtos;
using Loan.Management.Services.Payments.Dtos;
using Loan.Management.Services.Lenders.Dtos;

namespace Loan.Management.Profiles
{
    public class LoanManagementApplicationAutoMapperProfile : Profile
    {
        public LoanManagementApplicationAutoMapperProfile()
        {
            // ============ Borrower Mappings ============
            CreateMap<Borrower, BorrowerDto>();

            CreateMap<CreateBorrowerDto, Borrower>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LenderId, opt => opt.Ignore()) // Set by service
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Set to BorrowerStatus.New in entity constructor
                .ForMember(dest => dest.IsKycVerified, opt => opt.Ignore()) // Set in entity constructor
                .ForMember(dest => dest.KycVerificationDate, opt => opt.Ignore())
                .ForMember(dest => dest.TotalBorrowed, opt => opt.Ignore()) // Calculated from credits
                .ForMember(dest => dest.Outstanding, opt => opt.Ignore()) // Calculated from credits
                .ForMember(dest => dest.Paid, opt => opt.Ignore()) // Calculated from credits
                .ForMember(dest => dest.DefaultAmount, opt => opt.Ignore()) // Calculated from credits
                .ForMember(dest => dest.TotalLoans, opt => opt.Ignore()) // Calculated from credits
                .ForMember(dest => dest.DefaultedLoans, opt => opt.Ignore()) // Calculated from credits
                .ForMember(dest => dest.LastActivityDate, opt => opt.Ignore())
                .ForMember(dest => dest.Lender, opt => opt.Ignore())
                .ForMember(dest => dest.Credits, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            CreateMap<UpdateBorrowerDto, Borrower>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LenderId, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.IsKycVerified, opt => opt.Ignore())
                .ForMember(dest => dest.KycVerificationDate, opt => opt.Ignore())
                .ForMember(dest => dest.TotalBorrowed, opt => opt.Ignore())
                .ForMember(dest => dest.Outstanding, opt => opt.Ignore())
                .ForMember(dest => dest.Paid, opt => opt.Ignore())
                .ForMember(dest => dest.DefaultAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalLoans, opt => opt.Ignore())
                .ForMember(dest => dest.DefaultedLoans, opt => opt.Ignore())
                .ForMember(dest => dest.LastActivityDate, opt => opt.Ignore())
                .ForMember(dest => dest.Lender, opt => opt.Ignore())
                .ForMember(dest => dest.Credits, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            // ============ Credit Mappings ============
            CreateMap<Credit, CreditDto>()
                .ForMember(dest => dest.BorrowerName, opt => opt.MapFrom(src => src.Borrower.FullName))
                .ForMember(dest => dest.DaysOverdue, opt => opt.Ignore()) // Calculated in service
                .ForMember(dest => dest.PaymentProgress, opt => opt.Ignore()); // Calculated in service

            CreateMap<CreateCreditDto, Credit>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.LenderId, opt => opt.Ignore()) // Set by service
                .ForMember(dest => dest.Borrower, opt => opt.Ignore())
                .ForMember(dest => dest.Lender, opt => opt.Ignore())
                .ForMember(dest => dest.IssuedDate, opt => opt.Ignore()) // Set to DateTime.UtcNow in entity constructor
                .ForMember(dest => dest.InterestAmount, opt => opt.Ignore()) // Calculated by CalculateLoanAmounts()
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // Calculated by CalculateLoanAmounts()
                .ForMember(dest => dest.AmountPaid, opt => opt.Ignore()) // Set to 0 in entity constructor
                .ForMember(dest => dest.OutstandingAmount, opt => opt.Ignore()) // Calculated by CalculateLoanAmounts()
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Set to CreditStatus.Pending in constructor
                .ForMember(dest => dest.LastPaymentDate, opt => opt.Ignore());

            CreateMap<UpdateCreditDto, Credit>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BorrowerId, opt => opt.Ignore())
                .ForMember(dest => dest.LenderId, opt => opt.Ignore())
                .ForMember(dest => dest.Borrower, opt => opt.Ignore())
                .ForMember(dest => dest.Lender, opt => opt.Ignore())
                .ForMember(dest => dest.IssuedDate, opt => opt.Ignore())
                .ForMember(dest => dest.InterestAmount, opt => opt.Ignore()) // Recalculated by service
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore()) // Recalculated by service
                .ForMember(dest => dest.AmountPaid, opt => opt.Ignore())
                .ForMember(dest => dest.OutstandingAmount, opt => opt.Ignore()) // Recalculated by service
                .ForMember(dest => dest.LastPaymentDate, opt => opt.Ignore());

            // ============ Payment Mappings ============
            CreateMap<Payment, PaymentDto>();

            CreateMap<CreatePaymentDto, Payment>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BorrowerId, opt => opt.Ignore()) // Set by service
                .ForMember(dest => dest.BorrowerName, opt => opt.Ignore()) // Set by service
                .ForMember(dest => dest.PreviousOutstanding, opt => opt.Ignore()) // Set by service
                .ForMember(dest => dest.NewOutstanding, opt => opt.Ignore()); // Calculated by service

            // ============ Lender Mappings ============
            CreateMap<Lender, LenderDto>();

            CreateMap<CreateLenderDto, Lender>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.TotalLoansCreated, opt => opt.Ignore())
                .ForMember(dest => dest.TotalBorrowersManaged, opt => opt.Ignore());

            CreateMap<UpdateLenderDto, Lender>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.TotalLoansCreated, opt => opt.Ignore())
                .ForMember(dest => dest.TotalBorrowersManaged, opt => opt.Ignore());
        }
    }
}