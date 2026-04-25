using Money.Management.Entities;
using Money.Management.Enums;
using Money.Management.Services.Reports;
using Money.Management.Services.Reports.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Money.Management.Services
{
    public class ReportAppService : ApplicationService, IReportAppService
    {
        private readonly IRepository<Loan, Guid> _loanRepository;
        private readonly IRepository<LoanRepayment, Guid> _repaymentRepository;
        private readonly IRepository<Budget, Guid> _budgetRepository;
        private readonly IRepository<BudgetItem, Guid> _budgetItemRepository;

        public ReportAppService(
            IRepository<Loan, Guid> loanRepository,
            IRepository<LoanRepayment, Guid> repaymentRepository,
            IRepository<Budget, Guid> budgetRepository,
            IRepository<BudgetItem, Guid> budgetItemRepository)
        {
            _loanRepository = loanRepository;
            _repaymentRepository = repaymentRepository;
            _budgetRepository = budgetRepository;
            _budgetItemRepository = budgetItemRepository;
        }

        public async Task<LoanReportDto> GetLoanReportAsync(DateTime fromDate, DateTime toDate)
        {
            var loans = await _loanRepository.GetListAsync(
                l => l.StartDate >= fromDate && l.StartDate <= toDate);

            var loanIds = loans.Select(l => l.Id).ToList();
            var allRepayments = await _repaymentRepository.GetListAsync();
            var repayments = allRepayments
                .Where(r => loanIds.Contains(r.LoanId))
                .ToList();

            var trend = repayments
                .GroupBy(r => new { r.PaymentDate.Year, r.PaymentDate.Month })
                .Select(g => new MonthlyTrendDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthLabel = new DateTime(g.Key.Year, g.Key.Month, 1)
                        .ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Amount = g.Sum(r => r.AmountPaid),
                    Count = g.Count()
                })
                .OrderBy(t => t.Year).ThenBy(t => t.Month)
                .ToList();

            return new LoanReportDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalLoans = loans.Count,
                ActiveLoans = loans.Count(l => l.LoanStatus == LoanStatus.Active),
                CompletedLoans = loans.Count(l => l.LoanStatus == LoanStatus.Completed),
                DefaultedLoans = loans.Count(l => l.LoanStatus == LoanStatus.Defaulted),
                TotalDisbursed = loans.Sum(l => l.PrincipalAmount),
                TotalInterestEarned = loans
                    .Where(l => l.LoanStatus == LoanStatus.Completed)
                    .Sum(l => l.TotalAmount - l.PrincipalAmount),
                TotalOutstanding = loans.Sum(l => l.OutstandingBalance),
                TotalRepaid = repayments.Sum(r => r.AmountPaid),
                RepaymentTrend = trend
            };
        }

        public async Task<BudgetReportDto> GetBudgetReportAsync(int year)
        {
            var budgets = await _budgetRepository.GetListAsync(b => b.Year == year);
            var allItems = await _budgetItemRepository.GetListAsync();

            var monthlyBreakdown = new List<BudgetReportDto.MonthlySpendingDto>();
            var categoryMap = new Dictionary<string, (decimal Total, int Count)>();

            foreach (var budget in budgets.OrderBy(b => b.Month))
            {
                var items = allItems.Where(i => i.BudgetId == budget.Id).ToList();
                foreach (var item in items)
                    budget.Items.Add(item);

                var expenses = budget.GetTotalExpenses();
                var savings = budget.GetTotalSavings();

                monthlyBreakdown.Add(new BudgetReportDto.MonthlySpendingDto
                {
                    Month = budget.Month,
                    MonthLabel = new DateTime(year, budget.Month, 1)
                        .ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Salary = budget.Salary,
                    TotalExpenses = expenses,
                    TotalSavings = savings,
                    RemainingBalance = budget.GetRemainingBalance()
                });

                foreach (var item in items)
                {
                    if (!categoryMap.ContainsKey(item.Name))
                        categoryMap[item.Name] = (0, 0);
                    categoryMap[item.Name] = (
                        categoryMap[item.Name].Total + item.Amount,
                        categoryMap[item.Name].Count + 1);
                }
            }

            var categoryBreakdown = categoryMap
                .Select(kv => new BudgetReportDto.CategoryExpenseDto
                {
                    CategoryName = kv.Key,
                    TotalAmount = kv.Value.Total,
                    Count = kv.Value.Count
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();

            var savingsTrend = budgets
                .OrderBy(b => b.Month)
                .Select(b =>
                {
                    var items = allItems.Where(i => i.BudgetId == b.Id).ToList();
                    foreach (var item in items)
                        if (!b.Items.Contains(item))
                            b.Items.Add(item);

                    return new MonthlyTrendDto
                    {
                        Year = year,
                        Month = b.Month,
                        MonthLabel = new DateTime(year, b.Month, 1)
                            .ToString("MMM yyyy", CultureInfo.InvariantCulture),
                        Amount = b.GetTotalSavings(),
                        Count = b.Items.Count(i => i.Type == Enums.BudgetItemType.Saving)
                    };
                })
                .ToList();

            return new BudgetReportDto
            {
                Year = year,
                MonthlyBreakdown = monthlyBreakdown,
                CategoryBreakdown = categoryBreakdown,
                SavingsTrend = savingsTrend
            };
        }

        public async Task<ReportSummaryDto> GetSummaryReportAsync()
        {
            var loans = await _loanRepository.GetListAsync();
            var repayments = await _repaymentRepository.GetListAsync();
            var budgets = await _budgetRepository.GetListAsync();
            var budgetItems = await _budgetItemRepository.GetListAsync();

            var repaymentTrend = repayments
                .GroupBy(r => new { r.PaymentDate.Year, r.PaymentDate.Month })
                .Select(g => new MonthlyTrendDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthLabel = new DateTime(g.Key.Year, g.Key.Month, 1)
                        .ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Amount = g.Sum(r => r.AmountPaid),
                    Count = g.Count()
                })
                .OrderBy(t => t.Year).ThenBy(t => t.Month)
                .ToList();

            var budgetExpenseTrend = new List<MonthlyTrendDto>();
            foreach (var budget in budgets.OrderBy(b => b.Year).ThenBy(b => b.Month))
            {
                var items = budgetItems.Where(i => i.BudgetId == budget.Id).ToList();
                foreach (var item in items)
                    budget.Items.Add(item);

                budgetExpenseTrend.Add(new MonthlyTrendDto
                {
                    Year = budget.Year,
                    Month = budget.Month,
                    MonthLabel = new DateTime(budget.Year, budget.Month, 1)
                        .ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    Amount = budget.GetTotalExpenses(),
                    Count = budget.Items.Count(i => i.Type == Enums.BudgetItemType.Expense)
                });
            }

            var budgetCount = budgets.Count;

            return new ReportSummaryDto
            {
                TotalLoans = loans.Count,
                TotalDisbursed = loans.Sum(l => l.PrincipalAmount),
                TotalInterestEarned = loans
                    .Where(l => l.LoanStatus == LoanStatus.Completed)
                    .Sum(l => l.TotalAmount - l.PrincipalAmount),
                TotalOutstanding = loans.Sum(l => l.OutstandingBalance),
                TotalRepaid = repayments.Sum(r => r.AmountPaid),
                AverageMonthlySalary = budgetCount > 0
                    ? budgets.Average(b => b.Salary)
                    : 0,
                AverageMonthlyExpenses = budgetCount > 0
                    ? budgetExpenseTrend.Average(t => t.Amount)
                    : 0,
                AverageMonthlySavings = budgetCount > 0
                    ? budgets.Average(b =>
                    {
                        var items = budgetItems.Where(i => i.BudgetId == b.Id).ToList();
                        return items.Where(i => i.Type == Enums.BudgetItemType.Saving).Sum(i => i.Amount);
                    })
                    : 0,
                LoanRepaymentTrend = repaymentTrend,
                BudgetExpenseTrend = budgetExpenseTrend
            };
        }
    }
}
