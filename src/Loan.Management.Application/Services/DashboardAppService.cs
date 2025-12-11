using Loan.Management.DomainServices;
using Loan.Management.Entities;
using Loan.Management.Enums;
using Loan.Management.Permissions;
using Loan.Management.Services.Dashboards;
using Loan.Management.Services.Dashboards.Dtos;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Loan.Management.Services
{
    public class DashboardAppService : ApplicationService, IDashboardAppService
    {
        private readonly IRepository<Borrower, Guid> _borrowerRepository;
        private readonly IRepository<Credit, Guid> _creditRepository;
        private readonly IRepository<Payment, Guid> _paymentRepository;
        private readonly LenderManager _lenderManager;

        public DashboardAppService(
            IRepository<Borrower, Guid> borrowerRepository,
            IRepository<Credit, Guid> creditRepository,
            IRepository<Payment, Guid> paymentRepository,
            LenderManager lenderManager)
        {
            _borrowerRepository = borrowerRepository;
            _creditRepository = creditRepository;
            _paymentRepository = paymentRepository;
            _lenderManager = lenderManager;
        }

        private async Task<Guid> GetCurrentLenderIdAsync()
        {
            var lender = await _lenderManager.GetCurrentLenderAsync();
            return lender.Id;
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var dashboard = new DashboardDto
            {
                Summary = await GetSummaryAsync(),
                LoanDistribution = await GetLoanDistributionAsync(),
                TopBorrowers = await GetTopBorrowersAsync(10),
                MoneyFlow = await GetMoneyFlowAsync(12),
                RecentTransactions = await GetRecentTransactionsAsync(10),
                CollectionMetrics = await GetCollectionMetricsAsync(),
                UpcomingPayments = await GetUpcomingPaymentsAsync(30)
            };

            return dashboard;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var lenderId = await GetCurrentLenderIdAsync();

            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);
            var borrowers = await _borrowerRepository.GetListAsync(b => b.LenderId == lenderId);
            var payments = await _paymentRepository.GetListAsync(b => b.LenderId == lenderId);

            var activeLoans = credits.Where(l =>
                l.Status == CreditStatus.Active ||
                l.Status == CreditStatus.Overdue).ToList();

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            var lastMonth = DateTime.UtcNow.AddMonths(-1);

            // Calculate totals
            var totalLent = credits
                .Where(l => l.Status != CreditStatus.Rejected && l.Status != CreditStatus.Suspended)
                .Sum(l => l.TotalAmount);

            var totalOutstanding = activeLoans.Sum(l => l.OutstandingAmount);

            var totalCollected = credits
                .Where(l => l.Status != CreditStatus.Rejected && l.Status != CreditStatus.Suspended)
                .Sum(l => l.AmountPaid);

            var totalInterestEarned = credits
                .Where(l => l.Status != CreditStatus.Rejected && l.Status != CreditStatus.Suspended)
                .Sum(l => l.InterestAmount);

            // Calculate previous month totals for trends
            var previousMonthLent = credits
                .Where(l => l.IssuedDate.Month == lastMonth.Month &&
                           l.IssuedDate.Year == lastMonth.Year &&
                           l.Status != CreditStatus.Rejected && l.Status != CreditStatus.Suspended)
                .Sum(l => l.TotalAmount);

            var totalLentGrowth = previousMonthLent > 0
                ? ((totalLent - previousMonthLent) / previousMonthLent) * 100
                : 0;

            // Calculate rates
            var collectionRate = totalLent > 0
                ? (totalCollected / totalLent) * 100
                : 0;

            var defaultRate = credits.Count > 0
                ? (credits.Count(l => l.Status == CreditStatus.Defaulted) / (decimal)credits.Count) * 100
                : 0;

            var averageLoanSize = credits.Any()
                ? credits.Where(l => l.Status != CreditStatus.Rejected).Average(l => l.TotalAmount)
                : 0;

            var averageInterestRate = credits.Any()
                ? credits.Where(l => l.Status != CreditStatus.Rejected).Average(l => l.InterestRate)
                : 0;

            return new DashboardSummaryDto
            {
                TotalLent = totalLent,
                TotalOutstanding = totalOutstanding,
                TotalCollected = totalCollected,
                TotalInterestEarned = totalInterestEarned,

                ActiveLoansCount = credits.Count(l => l.Status == CreditStatus.Active),
                OverdueLoansCount = credits.Count(l => l.Status == CreditStatus.Overdue),
                PaidOffLoansCount = credits.Count(l => l.Status == CreditStatus.PaidOff),

                TotalBorrowersCount = borrowers.Count,
                ActiveBorrowersCount = borrowers.Count(b => b.Status == BorrowerStatus.Active),
                NewBorrowersThisMonth = borrowers.Count(b =>
                    b.CreationTime.Month == currentMonth &&
                    b.CreationTime.Year == currentYear),

                CollectionRate = collectionRate,
                DefaultRate = defaultRate,
                AverageLoanSize = averageLoanSize,
                AverageInterestRate = averageInterestRate,

                TotalLentGrowth = totalLentGrowth,
                NewLoansThisMonth = credits.Count(l =>
                    l.IssuedDate.Month == currentMonth &&
                    l.IssuedDate.Year == currentYear),
                NewPaymentsThisMonth = payments.Count(p =>
                    p.PaymentDate.Month == currentMonth &&
                    p.PaymentDate.Year == currentYear)
            };
        }

        public async Task<LoanStatusDistributionDto> GetLoanDistributionAsync()
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);

            var activeLoans = credits.Where(l => l.Status == CreditStatus.Active).ToList();
            var overdueLoans = credits.Where(l => l.Status == CreditStatus.Overdue).ToList();
            var paidOffLoans = credits.Where(l => l.Status == CreditStatus.PaidOff).ToList();
            var rejectedLoans = credits.Where(l => l.Status == CreditStatus.Rejected).ToList();
            var suspendedLoans = credits.Where(l => l.Status == CreditStatus.Suspended).ToList();

            var totalAmount = credits.Sum(l => l.TotalAmount);

            return new LoanStatusDistributionDto
            {
                ActiveCount = activeLoans.Count,
                OverdueCount = overdueLoans.Count,
                PaidOffCount = paidOffLoans.Count,
                CancelledCount = rejectedLoans.Count,
                WriteOffCount = suspendedLoans.Count,

                ActiveAmount = activeLoans.Sum(l => l.OutstandingAmount),
                OverdueAmount = overdueLoans.Sum(l => l.OutstandingAmount),
                PaidOffAmount = paidOffLoans.Sum(l => l.TotalAmount),
                CancelledAmount = rejectedLoans.Sum(l => l.TotalAmount),
                WriteOffAmount = suspendedLoans.Sum(l => l.TotalAmount),

                ActivePercentage = totalAmount > 0
                    ? (activeLoans.Sum(l => l.TotalAmount) / totalAmount) * 100
                    : 0,
                OverduePercentage = totalAmount > 0
                    ? (overdueLoans.Sum(l => l.TotalAmount) / totalAmount) * 100
                    : 0,
                PaidOffPercentage = totalAmount > 0
                    ? (paidOffLoans.Sum(l => l.TotalAmount) / totalAmount) * 100
                    : 0
            };
        }

        public async Task<List<TopBorrowerDto>> GetTopBorrowersAsync(int count = 10)
        {
            var lenderId = await GetCurrentLenderIdAsync();

            var borrowers = await _borrowerRepository.GetListAsync(b => b.LenderId == lenderId);
            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);
            var payments = await _paymentRepository.GetListAsync(p => p.LenderId == lenderId);

            var topBorrowers = new List<TopBorrowerDto>();

            foreach (var borrower in borrowers.OrderByDescending(b => b.TotalBorrowed).Take(count))
            {
                var borrowerLoans = credits.Where(l => l.BorrowerId == borrower.Id).ToList();
                var borrowerPayments = payments.Where(p => p.BorrowerId == borrower.Id).ToList();

                var activeLoansCount = borrowerLoans.Count(l =>
                    l.Status == CreditStatus.Active || l.Status == CreditStatus.Overdue);

                var lastPayment = borrowerPayments.OrderByDescending(p => p.PaymentDate).FirstOrDefault();
                var daysSinceLastPayment = lastPayment != null
                    ? (DateTime.UtcNow.Date - lastPayment.PaymentDate.Date).Days
                    : int.MaxValue;

                var paymentRate = borrower.TotalBorrowed > 0
                    ? (borrower.Paid / borrower.TotalBorrowed) * 100
                    : 0;

                var creditScore = CalculateCreditScore(borrower, borrowerLoans, paymentRate);

                topBorrowers.Add(new TopBorrowerDto
                {
                    BorrowerId = borrower.Id,
                    Name = borrower.FullName,
                    Phone = borrower.PhoneNumber,
                    TotalBorrowed = borrower.TotalBorrowed,
                    Outstanding = borrower.Outstanding,
                    Paid = borrower.Paid,
                    ActiveLoans = activeLoansCount,
                    TotalLoans = borrowerLoans.Count,
                    PaymentRate = paymentRate,
                    DaysSinceLastPayment = daysSinceLastPayment,
                    CreditScore = creditScore
                });
            }

            return topBorrowers;
        }

        public async Task<MoneyFlowDto> GetMoneyFlowAsync(int months = 12)
        {
            var lenderId = await GetCurrentLenderIdAsync();

            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);
            var payments = await _paymentRepository.GetListAsync(p => p.LenderId == lenderId);

            var monthlyData = new List<MonthlyFlowDto>();

            for (int i = months - 1; i >= 0; i--)
            {
                var targetDate = DateTime.UtcNow.AddMonths(-i);
                var month = targetDate.Month;
                var year = targetDate.Year;

                var monthLoans = credits
                    .Where(l => l.IssuedDate.Month == month &&
                               l.IssuedDate.Year == year &&
                               l.Status != CreditStatus.Rejected && l.Status != CreditStatus.Suspended)
                    .ToList();

                var monthPayments = payments
                    .Where(p => p.PaymentDate.Month == month &&
                                p.PaymentDate.Year == year)
                    .ToList();

                var lentAmount = monthLoans.Sum(l => l.TotalAmount);
                var collectedAmount = monthPayments.Sum(p => p.Amount);

                monthlyData.Add(new MonthlyFlowDto
                {
                    Month = targetDate.ToString("MMM"),
                    Year = year,
                    LentAmount = lentAmount,
                    CollectedAmount = collectedAmount,
                    NetFlow = collectedAmount - lentAmount,
                    LoansCount = monthLoans.Count,
                    PaymentsCount = monthPayments.Count
                });
            }

            var totalLentPeriod = monthlyData.Sum(m => m.LentAmount);
            var totalCollectedPeriod = monthlyData.Sum(m => m.CollectedAmount);

            return new MoneyFlowDto
            {
                MonthlyData = monthlyData,
                TotalLentPeriod = totalLentPeriod,
                TotalCollectedPeriod = totalCollectedPeriod,
                NetFlowPeriod = totalCollectedPeriod - totalLentPeriod,
                AverageMonthlyLending = monthlyData.Any() ? monthlyData.Average(m => m.LentAmount) : 0,
                AverageMonthlyCollection = monthlyData.Any() ? monthlyData.Average(m => m.CollectedAmount) : 0
            };
        }

        public async Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int count = 10)
        {
            var lenderId = await GetCurrentLenderIdAsync();

            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);
            var payments = await _paymentRepository.GetListAsync(p=>p.LenderId == lenderId);
            var borrowers = await _borrowerRepository.GetListAsync(b => b.LenderId == lenderId);

            var transactions = new List<RecentTransactionDto>();

            // Add recent credits
            foreach (var credit in credits.OrderByDescending(l => l.CreationTime).Take(count / 2))
            {
                var borrower = borrowers.FirstOrDefault(b => b.Id == credit.BorrowerId);
                transactions.Add(new RecentTransactionDto
                {
                    Id = credit.Id,
                    BorrowerName = borrower?.FullName ?? "Unknown",
                    Amount = credit.TotalAmount,
                    Date = credit.IssuedDate,
                    Type = "Credit",
                    Description = credit.Purpose ?? $"Credit of R{credit.PrincipalAmount:N2} at {credit.InterestRate}% interest",
                    Status = credit.Status.ToString()
                });
            }

            // Add recent payments
            foreach (var payment in payments.OrderByDescending(p => p.PaymentDate).Take(count / 2))
            {
                var credit = credits.FirstOrDefault(c => c.Id == payment.CreditId);
                if (credit == null) continue;

                var borrower = borrowers.FirstOrDefault(b => b.Id == credit.BorrowerId);
                transactions.Add(new RecentTransactionDto
                {
                    Id = payment.Id,
                    BorrowerName = borrower?.FullName ?? "Unknown",
                    Amount = payment.Amount,
                    Date = payment.PaymentDate,
                    Type = "Payment",
                    Description = payment.Description ?? $"Payment via {payment.Method}",
                    Status = "Completed"
                });
            }

            return transactions
                .OrderByDescending(t => t.Date)
                .Take(count)
                .ToList();
        }

        public async Task<CollectionMetricsDto> GetCollectionMetricsAsync()
        {
            var lenderId = await GetCurrentLenderIdAsync();

            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);
            var payments = await _paymentRepository.GetListAsync(p => p.LenderId == lenderId);

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;

            var monthPayments = payments
                .Where(p => p.PaymentDate.Month == currentMonth &&
                           p.PaymentDate.Year == currentYear)
                .ToList();

            var amountCollectedThisMonth = monthPayments.Sum(p => p.Amount);
            var paymentsThisMonth = monthPayments.Count;

            var targetCollection = credits
                .Where(l => l.DueDate.Month == currentMonth && l.DueDate.Year == currentYear)
                .Sum(l => l.OutstandingAmount);

            var collectionProgress = targetCollection > 0
                ? (amountCollectedThisMonth / targetCollection) * 100
                : 0;

            var paymentMethodDistribution = payments
                .GroupBy(p => p.Method)
                .Select(g => new PaymentMethodDistributionDto
                {
                    Method = g.Key.ToString(),
                    Count = g.Count(),
                    Amount = g.Sum(p => p.Amount),
                    Percentage = (g.Count() / (decimal)payments.Count) * 100
                })
                .OrderByDescending(p => p.Amount)
                .ToList();

            var totalPaid = credits.Sum(l => l.AmountPaid);
            var totalLent = credits.Where(l => l.Status != CreditStatus.Rejected && l.Status != CreditStatus.Suspended).Sum(l => l.TotalAmount);
            var collectionEfficiency = totalLent > 0 ? (totalPaid / totalLent) * 100 : 0;

            var onTimePayments = 0;
            var latePayments = 0;

            foreach (var payment in payments)
            {
                var credit = credits.FirstOrDefault(l => l.Id == payment.CreditId);
                if (credit != null)
                {
                    if (payment.PaymentDate <= credit.DueDate)
                        onTimePayments++;
                    else
                        latePayments++;
                }
            }

            var onTimeRate = (onTimePayments + latePayments) > 0
                ? (onTimePayments / (decimal)(onTimePayments + latePayments)) * 100
                : 0;

            var lateRate = (onTimePayments + latePayments) > 0
                ? (latePayments / (decimal)(onTimePayments + latePayments)) * 100
                : 0;

            return new CollectionMetricsDto
            {
                CollectionEfficiency = collectionEfficiency,
                OnTimePaymentRate = onTimeRate,
                LatePaymentRate = lateRate,
                PaymentsThisMonth = paymentsThisMonth,
                AmountCollectedThisMonth = amountCollectedThisMonth,
                TargetCollectionThisMonth = targetCollection,
                CollectionTargetProgress = collectionProgress,
                PaymentMethodDistribution = paymentMethodDistribution
            };
        }

        public async Task<List<UpcomingPaymentDto>> GetUpcomingPaymentsAsync(int days = 30)
        {
            var lenderId = await GetCurrentLenderIdAsync();

            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);
            var borrowers = await _borrowerRepository.GetListAsync(b => b.LenderId == lenderId);

            var today = DateTime.UtcNow.Date;
            var futureDate = today.AddDays(days);

            var upcomingLoans = credits
                .Where(l => (l.Status == CreditStatus.Active || l.Status == CreditStatus.Overdue) &&
                           l.OutstandingAmount > 0 &&
                           l.DueDate.Date >= today &&
                           l.DueDate.Date <= futureDate)
                .ToList();

            var upcomingPayments = new List<UpcomingPaymentDto>();

            foreach (var credit in upcomingLoans.OrderBy(l => l.DueDate))
            {
                var borrower = borrowers.FirstOrDefault(b => b.Id == credit.BorrowerId);
                if (borrower == null) continue;

                var daysUntilDue = (credit.DueDate.Date - today).Days;

                var priority = daysUntilDue <= 7 ? "High" :
                              daysUntilDue <= 14 ? "Medium" : "Low";

                upcomingPayments.Add(new UpcomingPaymentDto
                {
                    CreditId = credit.Id,
                    BorrowerId = borrower.Id,
                    BorrowerName = borrower.FullName,
                    BorrowerPhone = borrower.PhoneNumber,
                    OutstandingAmount = credit.OutstandingAmount,
                    DueDate = credit.DueDate,
                    DaysUntilDue = daysUntilDue,
                    Priority = priority
                });
            }

            return upcomingPayments;
        }

        public async Task<BorrowerAnalyticsDto> GetBorrowerAnalyticsAsync()
        {
            var lenderId = await GetCurrentLenderIdAsync();

            var borrowers = await _borrowerRepository.GetListAsync(b => b.LenderId == lenderId);
            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);

            var goodStanding = borrowers.Count(b => b.Outstanding == 0 || b.Status == BorrowerStatus.PaidOff);
            var atRisk = borrowers.Count(b => b.Outstanding > 0 && b.Status == BorrowerStatus.Active);
            var defaulted = borrowers.Count(b => b.Status == BorrowerStatus.Blacklisted || b.Status == BorrowerStatus.Suspended);

            var avgOutstanding = borrowers.Any()
                ? borrowers.Where(b => b.Outstanding > 0).Average(b => b.Outstanding)
                : 0;

            var completedLoans = credits.Where(l => l.Status == CreditStatus.PaidOff).ToList();
            var avgDuration = completedLoans.Any()
                ? completedLoans.Average(l => (l.DueDate - l.IssuedDate).Days)
                : 0;

            var segments = new List<BorrowerSegmentDto>
            {
                new BorrowerSegmentDto
                {
                    SegmentName = "Low Risk (< R5,000)",
                    Count = borrowers.Count(b => b.Outstanding < 5000),
                    TotalOutstanding = borrowers.Where(b => b.Outstanding < 5000).Sum(b => b.Outstanding),
                    Percentage = borrowers.Any() ? (borrowers.Count(b => b.Outstanding < 5000) / (decimal)borrowers.Count) * 100 : 0
                },
                new BorrowerSegmentDto
                {
                    SegmentName = "Medium Risk (R5,000 - R20,000)",
                    Count = borrowers.Count(b => b.Outstanding >= 5000 && b.Outstanding < 20000),
                    TotalOutstanding = borrowers.Where(b => b.Outstanding >= 5000 && b.Outstanding < 20000).Sum(b => b.Outstanding),
                    Percentage = borrowers.Any() ? (borrowers.Count(b => b.Outstanding >= 5000 && b.Outstanding < 20000) / (decimal)borrowers.Count) * 100 : 0
                },
                new BorrowerSegmentDto
                {
                    SegmentName = "High Risk (> R20,000)",
                    Count = borrowers.Count(b => b.Outstanding >= 20000),
                    TotalOutstanding = borrowers.Where(b => b.Outstanding >= 20000).Sum(b => b.Outstanding),
                    Percentage = borrowers.Any() ? (borrowers.Count(b => b.Outstanding >= 20000) / (decimal)borrowers.Count) * 100 : 0
                }
            };

            return new BorrowerAnalyticsDto
            {
                TotalBorrowers = borrowers.Count,
                GoodStandingBorrowers = goodStanding,
                AtRiskBorrowers = atRisk,
                DefaultedBorrowers = defaulted,
                AverageOutstandingPerBorrower = avgOutstanding,
                AverageLoanDuration = avgDuration,
                BorrowerSegments = segments
            };
        }

        public async Task<PortfolioHealthDto> GetPortfolioHealthAsync()
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);

            var totalOutstanding = credits
                .Where(l => l.Status == CreditStatus.Active || l.Status == CreditStatus.Overdue)
                .Sum(l => l.OutstandingAmount);

            var overdueLoans = credits.Where(l => l.Status == CreditStatus.Overdue).ToList();
            var overdueAmount = overdueLoans.Sum(l => l.OutstandingAmount);

            var portfolioAtRisk = totalOutstanding > 0
                ? (overdueAmount / totalOutstanding) * 100
                : 0;

            var par30Amount = overdueLoans.Where(l => (DateTime.UtcNow.Date - l.DueDate.Date).Days >= 30).Sum(l => l.OutstandingAmount);
            var par60Amount = overdueLoans.Where(l => (DateTime.UtcNow.Date - l.DueDate.Date).Days >= 60).Sum(l => l.OutstandingAmount);
            var par90Amount = overdueLoans.Where(l => (DateTime.UtcNow.Date - l.DueDate.Date).Days >= 90).Sum(l => l.OutstandingAmount);

            var par30 = totalOutstanding > 0 ? (par30Amount / totalOutstanding) * 100 : 0;
            var par60 = totalOutstanding > 0 ? (par60Amount / totalOutstanding) * 100 : 0;
            var par90 = totalOutstanding > 0 ? (par90Amount / totalOutstanding) * 100 : 0;

            var writeOffRatio = credits.Any()
                ? (credits.Count(l => l.Status == CreditStatus.Defaulted) / (decimal)credits.Count) * 100
                : 0;

            var delinquencyRate = credits.Any()
                ? (credits.Count(l => l.Status == CreditStatus.Overdue) / (decimal)credits.Count) * 100
                : 0;

            var overallHealth = portfolioAtRisk < 5 ? "Excellent" :
                               portfolioAtRisk < 10 ? "Good" :
                               portfolioAtRisk < 20 ? "Fair" : "Poor";

            var healthIndicators = new List<string>();
            var recommendations = new List<string>();

            if (portfolioAtRisk < 5)
                healthIndicators.Add("✓ Portfolio at Risk is excellent (< 5%)");
            else if (portfolioAtRisk > 20)
            {
                healthIndicators.Add("✗ Portfolio at Risk is concerning (> 20%)");
                recommendations.Add("Review collection strategies and borrower creditworthiness");
            }

            if (par90 > 10)
            {
                healthIndicators.Add("✗ High PAR90 indicates severe delinquency issues");
                recommendations.Add("Consider debt restructuring or write-offs for severely overdue credits");
            }

            if (delinquencyRate < 10)
                healthIndicators.Add("✓ Low delinquency rate indicates strong borrower quality");

            return new PortfolioHealthDto
            {
                PortfolioAtRisk = portfolioAtRisk,
                Par30 = par30,
                Par60 = par60,
                Par90 = par90,
                WriteOffRatio = writeOffRatio,
                DelinquencyRate = delinquencyRate,
                OverallHealth = overallHealth,
                HealthIndicators = healthIndicators,
                Recommendations = recommendations
            };
        }

        public async Task<LoanAgingDto> GetLoanAgingAsync()
        {
            var lenderId = await GetCurrentLenderIdAsync();
            var credits = await _creditRepository.GetListAsync(c => c.LenderId == lenderId);
            var today = DateTime.UtcNow.Date;

            var current = credits.Where(l =>
                l.Status == CreditStatus.Active &&
                (l.DueDate.Date - today).Days >= 0 &&
                (l.DueDate.Date - today).Days <= 30).ToList();

            var days30_60 = credits.Where(l =>
                (today - l.DueDate.Date).Days >= 30 &&
                (today - l.DueDate.Date).Days < 60).ToList();

            var days60_90 = credits.Where(l =>
                (today - l.DueDate.Date).Days >= 60 &&
                (today - l.DueDate.Date).Days < 90).ToList();

            var days90Plus = credits.Where(l =>
                (today - l.DueDate.Date).Days >= 90).ToList();

            return new LoanAgingDto
            {
                Current = current.Count,
                Days30_60 = days30_60.Count,
                Days60_90 = days60_90.Count,
                Days90Plus = days90Plus.Count,
                CurrentAmount = current.Sum(l => l.OutstandingAmount),
                Days30_60Amount = days30_60.Sum(l => l.OutstandingAmount),
                Days60_90Amount = days60_90.Sum(l => l.OutstandingAmount),
                Days90PlusAmount = days90Plus.Sum(l => l.OutstandingAmount)
            };
        }

        public async Task<Dictionary<string, decimal>> GetKpisAsync()
        {
            var summary = await GetSummaryAsync();
            var portfolioHealth = await GetPortfolioHealthAsync();
            var collectionMetrics = await GetCollectionMetricsAsync();

            return new Dictionary<string, decimal>
            {
                { "TotalLent", summary.TotalLent },
                { "TotalOutstanding", summary.TotalOutstanding },
                { "CollectionRate", summary.CollectionRate },
                { "PortfolioAtRisk", portfolioHealth.PortfolioAtRisk },
                { "DelinquencyRate", portfolioHealth.DelinquencyRate },
                { "CollectionEfficiency", collectionMetrics.CollectionEfficiency },
                { "ActiveLoans", summary.ActiveLoansCount },
                { "OverdueLoans", summary.OverdueLoansCount }
            };
        }

        public async Task<byte[]> ExportDashboardDataAsync()
        {
            var dashboard = await GetDashboardDataAsync();

            var jsonData = System.Text.Json.JsonSerializer.Serialize(dashboard);
            return System.Text.Encoding.UTF8.GetBytes(jsonData);
        }

        private string CalculateCreditScore(Borrower borrower, List<Credit> credits, decimal paymentRate)
        {
            var score = 0;

            if (paymentRate >= 90) score += 40;
            else if (paymentRate >= 70) score += 30;
            else if (paymentRate >= 50) score += 20;
            else score += 10;

            var overdueLoans = credits.Count(l => l.Status == CreditStatus.Overdue);
            if (overdueLoans == 0) score += 30;
            else if (overdueLoans == 1) score += 20;
            else if (overdueLoans == 2) score += 10;

            if (borrower.TotalBorrowed >= 50000) score += 30;
            else if (borrower.TotalBorrowed >= 20000) score += 20;
            else if (borrower.TotalBorrowed >= 10000) score += 10;

            if (score >= 80) return "Good";
            if (score >= 50) return "Fair";
            return "Poor";
        }
    }
}