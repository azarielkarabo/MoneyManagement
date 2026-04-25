using System.Collections.Generic;

namespace Money.Management.Services.Dashboards.Dtos
{
    /// <summary>
    /// Portfolio health metrics
    /// </summary>
    public class PortfolioHealthDto
    {
        public decimal PortfolioAtRisk { get; set; } // PAR - percentage of overdue credits
        public decimal Par30 { get; set; } // credits overdue > 30 days
        public decimal Par60 { get; set; } // credits overdue > 60 days
        public decimal Par90 { get; set; } // credits overdue > 90 days

        public decimal WriteOffRatio { get; set; }
        public decimal DelinquencyRate { get; set; }

        public string OverallHealth { get; set; } // "Excellent", "Good", "Fair", "Poor"
        public List<string> HealthIndicators { get; set; }
        public List<string> Recommendations { get; set; }

        public PortfolioHealthDto()
        {
            HealthIndicators = new List<string>();
            Recommendations = new List<string>();
        }
    }
}