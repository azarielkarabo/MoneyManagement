using Money.Management.DomainServices;
using Money.Management.Entities;
using Money.Management.Enums;
using Money.Management.Services.Loans;
using Money.Management.Services.Loans.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Money.Management.Services
{
    public class LoanAppService : ApplicationService, ILoanAppService
    {
        private readonly IRepository<Loan, Guid> _loanRepository;
        private readonly IRepository<LoanRepayment, Guid> _repaymentRepository;
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly LoanManager _loanManager;

        public LoanAppService(
            IRepository<Loan, Guid> loanRepository,
            IRepository<LoanRepayment, Guid> repaymentRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            LoanManager loanManager)
        {
            _loanRepository = loanRepository;
            _repaymentRepository = repaymentRepository;
            _borrowerRepository = borrowerRepository;
            _loanManager = loanManager;
        }

        public async Task<LoanDto> GetLoanAsync(Guid id)
        {
            var loan = await _loanRepository.GetAsync(id);
            return await MapToLoanDtoAsync(loan);
        }

        public async Task<PagedResultDto<LoanDto>> GetLoansAsync(GetLoansInput input)
        {
            var queryable = await _loanRepository.GetQueryableAsync();

            if (input.BorrowerId.HasValue)
                queryable = queryable.Where(l => l.BorrowerId == input.BorrowerId.Value);

            if (input.Status.HasValue)
                queryable = queryable.Where(l => l.LoanStatus == input.Status.Value);

            if (input.FromDate.HasValue)
                queryable = queryable.Where(l => l.StartDate >= input.FromDate.Value);

            if (input.ToDate.HasValue)
                queryable = queryable.Where(l => l.StartDate <= input.ToDate.Value);

            var totalCount = await AsyncExecuter.CountAsync(queryable);

            queryable = queryable
                .OrderByDescending(l => l.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount);

            var loans = await AsyncExecuter.ToListAsync(queryable);

            var dtos = new List<LoanDto>();
            foreach (var loan in loans)
                dtos.Add(await MapToLoanDtoAsync(loan));

            return new PagedResultDto<LoanDto>(totalCount, dtos);
        }

        public async Task<LoanDto> CreateLoanAsync(CreateLoanDto input)
        {
            var loan = await _loanManager.CreateAsync(
                input.BorrowerId,
                input.PrincipalAmount,
                input.InterestRate,
                input.StartDate,
                input.EndDate,
                input.Notes);

            await _loanRepository.InsertAsync(loan);
            await CurrentUnitOfWork.SaveChangesAsync();

            return await GetLoanAsync(loan.Id);
        }

        public async Task<LoanRepaymentDto> MakeRepaymentAsync(Guid loanId, MakeRepaymentDto input)
        {
            var repayment = await _loanManager.MakeRepaymentAsync(
                loanId,
                input.AmountPaid,
                input.PaymentDate);

            repayment.Notes = input.Notes;

            await _repaymentRepository.InsertAsync(repayment);
            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<LoanRepayment, LoanRepaymentDto>(repayment);
        }

        public async Task<PagedResultDto<LoanRepaymentDto>> GetLoanRepaymentsAsync(Guid loanId)
        {
            var repayments = await _repaymentRepository.GetListAsync(r => r.LoanId == loanId);
            repayments = repayments.OrderByDescending(r => r.PaymentDate).ToList();

            var dtos = ObjectMapper.Map<List<LoanRepayment>, List<LoanRepaymentDto>>(repayments);
            return new PagedResultDto<LoanRepaymentDto>(repayments.Count, dtos);
        }

        public async Task<LoanSummaryReportDto> GetLoanSummaryReportAsync()
        {
            var loans = await _loanRepository.GetListAsync();
            var repayments = await _repaymentRepository.GetListAsync();

            return new LoanSummaryReportDto
            {
                TotalLoans = loans.Count,
                ActiveLoans = loans.Count(l => l.LoanStatus == LoanStatus.Active),
                CompletedLoans = loans.Count(l => l.LoanStatus == LoanStatus.Completed),
                DefaultedLoans = loans.Count(l => l.LoanStatus == LoanStatus.Defaulted),
                TotalPrincipal = loans.Sum(l => l.PrincipalAmount),
                TotalInterestExpected = loans.Sum(l => l.TotalAmount - l.PrincipalAmount),
                TotalAmount = loans.Sum(l => l.TotalAmount),
                TotalPaid = repayments.Sum(r => r.AmountPaid),
                TotalOutstanding = loans.Sum(l => l.OutstandingBalance)
            };
        }

        private async Task<LoanDto> MapToLoanDtoAsync(Loan loan)
        {
            var dto = ObjectMapper.Map<Loan, LoanDto>(loan);

            var borrower = await _borrowerRepository.FindAsync(loan.BorrowerId);
            dto.BorrowerName = borrower?.FullName;

            var repayments = await _repaymentRepository.GetListAsync(r => r.LoanId == loan.Id);
            dto.TotalPaid = repayments.Sum(r => r.AmountPaid);

            dto.RepaymentProgress = loan.TotalAmount > 0
                ? (dto.TotalPaid / loan.TotalAmount) * 100
                : 0;

            return dto;
        }
    }
}
