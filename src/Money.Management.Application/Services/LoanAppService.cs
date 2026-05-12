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
        private readonly LoanCalculationService _calculationService;

        public LoanAppService(
            IRepository<Loan, Guid> loanRepository,
            IRepository<LoanRepayment, Guid> repaymentRepository,
            IRepository<Borrower, Guid> borrowerRepository,
            LoanManager loanManager,
            LoanCalculationService calculationService)
        {
            _loanRepository = loanRepository;
            _repaymentRepository = repaymentRepository;
            _borrowerRepository = borrowerRepository;
            _loanManager = loanManager;
            _calculationService = calculationService;
        }

        // ── CRUD ──────────────────────────────────────────────────────────────────

        public async Task<LoanDto> GetLoanAsync(Guid id)
        {
            var loan = await _loanRepository.GetAsync(id);
            return await ToLoanDtoAsync(loan);
        }

        public async Task<PagedResultDto<LoanDto>> GetLoansAsync(GetLoansInput input)
        {
            var queryable = await _loanRepository.GetQueryableAsync();

            if (input.BorrowerId.HasValue)
                queryable = queryable.Where(l => l.BorrowerId == input.BorrowerId.Value);

            if (input.Status.HasValue)
                queryable = queryable.Where(l => l.Status == input.Status.Value);

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

            var dtos = new List<LoanDto>(loans.Count);
            foreach (var loan in loans)
                dtos.Add(await ToLoanDtoAsync(loan));

            return new PagedResultDto<LoanDto>(totalCount, dtos);
        }

        public async Task<LoanDto> CreateLoanAsync(CreateLoanDto input)
        {
            // LoanManager verifies the borrower exists and delegates construction to the entity
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

        public async Task DeleteLoanAsync(Guid id)
        {
            var loan = await _loanRepository.GetAsync(id);

            var hasRepayments = await _repaymentRepository.AnyAsync(r => r.LoanId == id);
            if (hasRepayments)
                throw new InvalidOperationException(
                    "Cannot delete a loan that has repayment records.");

            await _loanRepository.DeleteAsync(id);
        }

        // ── Workflow ──────────────────────────────────────────────────────────────

        public async Task<LoanDto> ApproveLoanAsync(Guid id)
        {
            var loan = await _loanManager.ApproveAsync(id);
            return await ToLoanDtoAsync(loan);
        }

        public async Task<LoanDto> MarkAsDefaultedAsync(Guid id)
        {
            await _loanManager.MarkAsDefaultedAsync(id);
            return await GetLoanAsync(id);
        }

        // ── Repayments ────────────────────────────────────────────────────────────

        public async Task<LoanRepaymentDto> MakeRepaymentAsync(Guid loanId, MakeRepaymentDto input)
        {
            var repayment = await _loanManager.MakeRepaymentAsync(
                loanId,
                input.AmountPaid,
                input.PaymentDate,
                input.Notes);

            // Repayment is persisted via cascade from the loan aggregate update
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

        // ── Reports ───────────────────────────────────────────────────────────────

        public async Task<LoanSummaryReportDto> GetLoanSummaryReportAsync()
        {
            var loans = await _loanRepository.GetListAsync();
            var repayments = await _repaymentRepository.GetListAsync();

            return new LoanSummaryReportDto
            {
                TotalLoans = loans.Count,
                ActiveLoans = loans.Count(l => l.Status == LoanStatus.Active),
                CompletedLoans = loans.Count(l => l.Status == LoanStatus.Completed),
                DefaultedLoans = loans.Count(l => l.Status == LoanStatus.Defaulted),
                TotalPrincipal = loans.Sum(l => l.PrincipalAmount),
                TotalInterestExpected = loans.Sum(l => l.InterestAmount),
                TotalAmount = loans.Sum(l => l.TotalAmount),
                TotalPaid = repayments.Sum(r => r.AmountPaid),
                TotalOutstanding = loans.Sum(l => l.OutstandingBalance)
            };
        }

        // ── Calculations ──────────────────────────────────────────────────────────

        public Task<LoanCalculationDto> CalculateLoanAsync(LoanCalculationInput input)
        {
            var termYears = input.TermMonths / 12m;

            var result = new LoanCalculationDto
            {
                PrincipalAmount = input.PrincipalAmount,
                AnnualInterestRate = input.AnnualInterestRate,
                TermMonths = input.TermMonths,
                SimpleInterest = _calculationService.CalculateSimpleInterest(
                    input.PrincipalAmount, input.AnnualInterestRate, termYears),
                TotalAmountSimple = _calculationService.CalculateTotalAmountSimple(
                    input.PrincipalAmount, input.AnnualInterestRate, termYears),
                CompoundInterest = _calculationService.CalculateCompoundInterest(
                    input.PrincipalAmount, input.AnnualInterestRate, termYears),
                TotalAmountCompound = _calculationService.CalculateTotalAmountCompound(
                    input.PrincipalAmount, input.AnnualInterestRate, termYears),
                MonthlyInstalment = _calculationService.CalculateMonthlyInstalment(
                    input.PrincipalAmount, input.AnnualInterestRate, input.TermMonths)
            };

            return Task.FromResult(result);
        }

        public Task<AmortizationScheduleDto> GetAmortizationScheduleAsync(
            AmortizationScheduleInput input)
        {
            var schedule = _calculationService.GenerateAmortizationSchedule(
                input.PrincipalAmount,
                input.AnnualInterestRate,
                input.TermMonths,
                input.StartDate);

            var dto = new AmortizationScheduleDto
            {
                Principal = schedule.Principal,
                AnnualRatePct = schedule.AnnualRatePct,
                TermMonths = schedule.TermMonths,
                MonthlyInstalment = schedule.MonthlyInstalment,
                TotalPayment = schedule.TotalPayment,
                TotalInterest = schedule.TotalInterest,
                Entries = schedule.Entries.Select(e => new AmortizationEntryDto
                {
                    Month = e.Month,
                    DueDate = e.DueDate,
                    OpeningBalance = e.OpeningBalance,
                    Emi = e.Emi,
                    InterestPortion = e.InterestPortion,
                    PrincipalPortion = e.PrincipalPortion,
                    ClosingBalance = e.ClosingBalance
                }).ToList()
            };

            return Task.FromResult(dto);
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private async Task<LoanDto> ToLoanDtoAsync(Loan loan)
        {
            var dto = ObjectMapper.Map<Loan, LoanDto>(loan);

            var borrower = await _borrowerRepository.FindAsync(loan.BorrowerId);
            dto.BorrowerName = borrower?.FullName;

            var repayments = await _repaymentRepository.GetListAsync(r => r.LoanId == loan.Id);
            dto.TotalPaid = repayments.Sum(r => r.AmountPaid);
            dto.RepaymentProgress = loan.TotalAmount > 0
                ? (dto.TotalPaid / loan.TotalAmount) * 100m
                : 0m;

            return dto;
        }
    }
}
