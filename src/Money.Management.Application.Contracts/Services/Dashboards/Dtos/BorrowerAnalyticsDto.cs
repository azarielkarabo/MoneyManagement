using System.Collections.Generic;

namespace Money.Management.Services.Dashboards.Dtos
{
    /// <summary>
    /// Borrower performance analytics
    /// </summary>
    public class BorrowerAnalyticsDto
    {
        public int TotalBorrowers { get; set; }
        public int GoodStandingBorrowers { get; set; }
        public int AtRiskBorrowers { get; set; }
        public int DefaultedBorrowers { get; set; }

        public decimal AverageOutstandingPerBorrower { get; set; }
        public double AverageMoneyDuration { get; set; }

        public List<BorrowerSegmentDto> BorrowerSegments { get; set; }

        public BorrowerAnalyticsDto()
        {
            BorrowerSegments = new List<BorrowerSegmentDto>();
        }
    }
}