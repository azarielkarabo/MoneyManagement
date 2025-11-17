using AutoMapper;
using Loan.Management.Entities;
using Loan.Management.Services.Borrowers.Dtos;
using Loan.Management.Services.Loans.Dtos;
using Loan.Management.Services.Payments.Dtos;

namespace Loan.Management.Profiles
{
    namespace LoanManagement
    {
        public class LoanManagementApplicationAutoMapperProfile : Profile
        {
            public LoanManagementApplicationAutoMapperProfile()
            {
                // Borrower Mappings
                CreateMap<Borrower, BorrowerDto>();
                CreateMap<CreateBorrowerDto, Borrower>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.Loans, opt => opt.Ignore())
                    .ForMember(dest => dest.Loans, opt => opt.Ignore())
                    .ForMember(dest => dest.Payments, opt => opt.Ignore());

                // Loan Mappings
                CreateMap<Entities.Loan, LoanDto>()
                    .ForMember(dest => dest.DaysOverdue, opt => opt.MapFrom(src => src.DaysOverdue))
                    .ForMember(dest => dest.OverdueSeverity, opt => opt.MapFrom(src => src.OverdueSeverity))
                    .ForMember(dest => dest.PaymentProgress, opt => opt.Ignore()); // Calculated in service

                CreateMap<CreateLoanDto, Entities.Loan>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.BorrowerName, opt => opt.Ignore())
                    .ForMember(dest => dest.Payments, opt => opt.Ignore());

                CreateMap<UpdateLoanDto, Entities.Loan>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.BorrowerId, opt => opt.Ignore())
                    .ForMember(dest => dest.BorrowerName, opt => opt.Ignore())
                    .ForMember(dest => dest.Payments, opt => opt.Ignore());

                // Payment Mappings
                CreateMap<Payment, PaymentDto>();

                CreateMap<CreatePaymentDto, Payment>()
                    .ForMember(dest => dest.Id, opt => opt.Ignore())
                    .ForMember(dest => dest.BorrowerId, opt => opt.Ignore())
                    .ForMember(dest => dest.BorrowerName, opt => opt.Ignore())
                    .ForMember(dest => dest.PreviousOutstanding, opt => opt.Ignore())
                    .ForMember(dest => dest.NewOutstanding, opt => opt.Ignore());
            }
        }
    }
}