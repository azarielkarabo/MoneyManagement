using Money.Management.DomainServices;
using Money.Management.Entities;
using Money.Management.Enums;
using Money.Management.Permissions;
using Money.Management.Services.Reports;
using Money.Management.Services.Reports.Dtos;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly IRepository<Payment, Guid> _repaymentRepository;
        private readonly LenderManager _lenderManager;
        private readonly IRepository<Budget, Guid> _budgetRepository;
        private readonly IRepository<BudgetItem, Guid> _budgetItemRepository;

        public ReportAppService(
            IRepository<Credit, Guid> creditRepository,
            IRepository<Payment, Guid> repaymentRepository,
            LenderManager lenderManager,
            IRepository<Budget, Guid> budgetRepository,
            IRepository<BudgetItem, Guid> budgetItemRepository)
        {
            _creditRepository = creditRepository;
            _lenderManager = lenderManager;
            _repaymentRepository = repaymentRepository;
            _budgetRepository = budgetRepository;
            _budgetItemRepository = budgetItemRepository;
        }

        // ── Loan report ────────────────────────────────────────────────────────────


        // Admins see everything (null = no lender filter); lenders only see their own data.
        private async Task<Guid?> GetLenderScopeAsync()
        {
            if (CurrentUser.IsInRole("admin"))
            {
                return null;
            }
            return (await _lenderManager.GetCurrentLenderAsync()).Id;
        }

        [Authorize(MoneyManagementPermissions.Reports.Generate)]
        public async Task<LoanReportDto> GetLoanReportAsync(DateTime fromDate, DateTime toDate)
        {
            var lenderId = await GetLenderScopeAsync();
            var loans = await _creditRepository.GetListAsync(
                l => (lenderId == null || l.LenderId == lenderId) && l.IssuedDate >= fromDate && l.IssuedDate <= toDate);

            var loanIds = loans.Select(l => l.Id).ToHashSet();
            var allRepayments = await _repaymentRepository.GetListAsync(
                p => (lenderId == null || p.LenderId == lenderId) && p.PaymentDate >= fromDate && p.PaymentDate <= toDate);
            var repayments = allRepayments.Where(r => loanIds.Contains(r.CreditId)).ToList();

            // Interest earned = sum of InterestAmount on fully completed loans only
            var interestEarned = loans
                .Where(l => l.Status == CreditStatus.PaidOff)
                .Sum(l => l.InterestAmount);

            // Repayment trend grouped by month
            var trend = repayments
                .GroupBy(r => new { r.PaymentDate.Year, r.PaymentDate.Month })
                .Select(g => new MonthlyTrendDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthLabel = MonthLabel(g.Key.Year, g.Key.Month),
                    Amount = g.Sum(r => r.Amount),
                    Count = g.Count()
                })
                .OrderBy(t => t.Year).ThenBy(t => t.Month)
                .ToList();

            return new LoanReportDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalLoans = loans.Count,
                ActiveLoans = loans.Count(l => l.Status == CreditStatus.Active || l.Status == CreditStatus.Overdue),
                CompletedLoans = loans.Count(l => l.Status == CreditStatus.PaidOff),
                DefaultedLoans = loans.Count(l => l.Status == CreditStatus.Defaulted),
                TotalDisbursed = loans.Sum(l => l.PrincipalAmount),
                TotalInterestEarned = interestEarned,
                TotalOutstanding = loans.Sum(l => l.OutstandingAmount),
                TotalRepaid = repayments.Sum(r => r.Amount),
                RepaymentTrend = trend
            };
        }

        // ── Budget report (monthly breakdown + category + savings trend) ────────────

        [Authorize(MoneyManagementPermissions.Reports.Generate)]
        public async Task<BudgetReportDto> GetBudgetReportAsync(int year)
        {
            var budgets = await _budgetRepository.GetListAsync(b => b.Year == year);
            var allItems = await _budgetItemRepository.GetListAsync();

            var monthlyBreakdown = new List<BudgetReportDto.MonthlySpendingDto>();
            var categoryTotals = new Dictionary<string, (decimal Planned, decimal Actual, int Count)>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var budget in budgets.OrderBy(b => b.Month))
            {
                var items = allItems.Where(i => i.BudgetId == budget.Id).ToList();

                var plannedExpenses = items
                    .Where(i => i.Type == BudgetItemType.Expense).Sum(i => i.Amount);
                var actualExpenses = items
                    .Where(i => i.Type == BudgetItemType.Expense).Sum(i => i.ActualAmount ?? 0m);
                var plannedSavings = items
                    .Where(i => i.Type == BudgetItemType.Saving).Sum(i => i.Amount);
                var actualSavings = items
                    .Where(i => i.Type == BudgetItemType.Saving).Sum(i => i.ActualAmount ?? 0m);

                monthlyBreakdown.Add(new BudgetReportDto.MonthlySpendingDto
                {
                    Month = budget.Month,
                    MonthLabel = MonthLabel(year, budget.Month),
                    Salary = budget.Salary,
                    TotalExpenses = plannedExpenses,
                    TotalSavings = plannedSavings,
                    RemainingBalance = budget.Salary - actualExpenses,

                    // Planned vs Actual
                    TotalActualExpenses = actualExpenses,
                    TotalActualSavings = actualSavings,
                    Variance = plannedExpenses - actualExpenses,
                    IsOverspending = actualExpenses > budget.Salary
                });

                // Category roll-up
                foreach (var item in items)
                {
                    if (!categoryTotals.ContainsKey(item.Name))
                        categoryTotals[item.Name] = (0m, 0m, 0);

                    var (p, a, c) = categoryTotals[item.Name];
                    categoryTotals[item.Name] = (
                        p + item.Amount,
                        a + (item.ActualAmount ?? 0m),
                        c + 1);
                }
            }

            var categoryBreakdown = categoryTotals
                .Select(kv => new BudgetReportDto.CategoryExpenseDto
                {
                    CategoryName = kv.Key,
                    TotalAmount = kv.Value.Planned,
                    TotalActualAmount = kv.Value.Actual,
                    Count = kv.Value.Count
                })
                .OrderByDescending(c => c.TotalAmount)
                .ToList();

            var savingsTrend = monthlyBreakdown
                .Select(m => new MonthlyTrendDto
                {
                    Year = year,
                    Month = m.Month,
                    MonthLabel = m.MonthLabel,
                    Amount = m.TotalActualSavings > 0 ? m.TotalActualSavings : m.TotalSavings,
                    Count = 0
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

        // ── Cross-module summary ───────────────────────────────────────────────────

        [Authorize(MoneyManagementPermissions.Reports.Generate)]
        public async Task<ReportSummaryDto> GetSummaryReportAsync()
        {
            var lenderId = await GetLenderScopeAsync();
            var loans = await _creditRepository.GetListAsync(l => (lenderId == null || l.LenderId == lenderId));
            var repayments = await _repaymentRepository.GetListAsync(p => (lenderId == null || p.LenderId == lenderId));
            var budgets = await _budgetRepository.GetListAsync();
            var budgetItems = await _budgetItemRepository.GetListAsync();

            // Loan repayment trend
            var loanTrend = repayments
                .GroupBy(r => new { r.PaymentDate.Year, r.PaymentDate.Month })
                .Select(g => new MonthlyTrendDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthLabel = MonthLabel(g.Key.Year, g.Key.Month),
                    Amount = g.Sum(r => r.Amount),
                    Count = g.Count()
                })
                .OrderBy(t => t.Year).ThenBy(t => t.Month)
                .ToList();

            // Budget expense trend (actual where available, else planned)
            var budgetTrend = budgets
                .OrderBy(b => b.Year).ThenBy(b => b.Month)
                .Select(b =>
                {
                    var items = budgetItems.Where(i => i.BudgetId == b.Id).ToList();
                    var actualExpenses = items
                        .Where(i => i.Type == BudgetItemType.Expense)
                        .Sum(i => i.ActualAmount ?? i.Amount);

                    return new MonthlyTrendDto
                    {
                        Year = b.Year,
                        Month = b.Month,
                        MonthLabel = MonthLabel(b.Year, b.Month),
                        Amount = actualExpenses,
                        Count = items.Count(i => i.Type == BudgetItemType.Expense)
                    };
                })
                .ToList();

            var budgetCount = budgets.Count;

            return new ReportSummaryDto
            {
                TotalLoans = loans.Count,
                TotalDisbursed = loans.Sum(l => l.PrincipalAmount),
                TotalInterestEarned = loans
                    .Where(l => l.Status == CreditStatus.PaidOff)
                    .Sum(l => l.InterestAmount),
                TotalOutstanding = loans.Sum(l => l.OutstandingAmount),
                TotalRepaid = repayments.Sum(r => r.Amount),
                AverageMonthlySalary = budgetCount > 0 ? budgets.Average(b => b.Salary) : 0m,
                AverageMonthlyExpenses = budgetTrend.Count > 0
                    ? budgetTrend.Average(t => t.Amount) : 0m,
                AverageMonthlySavings = budgetCount > 0
                    ? budgets.Average(b =>
                    {
                        var items = budgetItems.Where(i => i.BudgetId == b.Id).ToList();
                        return items
                            .Where(i => i.Type == BudgetItemType.Saving)
                            .Sum(i => i.ActualAmount ?? i.Amount);
                    })
                    : 0m,
                LoanRepaymentTrend = loanTrend,
                BudgetExpenseTrend = budgetTrend
            };
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static string MonthLabel(int year, int month) =>
            new DateTime(year, month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture);
    }
}
