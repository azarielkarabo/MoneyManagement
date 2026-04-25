using System;
using System.Collections.Generic;

namespace Money.Management.Services.Dashboards.Dtos
{
    /// <summary>
    /// Money flow analysis
    /// </summary>
    public class MoneyFlowDto
    {
        public List<MonthlyFlowDto> MonthlyData { get; set; }
        public decimal TotalLentPeriod { get; set; }
        public decimal TotalCollectedPeriod { get; set; }
        public decimal NetFlowPeriod { get; set; }
        public decimal AverageMonthlyLending { get; set; }
        public decimal AverageMonthlyCollection { get; set; }

        public MoneyFlowDto()
        {
            MonthlyData = new List<MonthlyFlowDto>();
        }
    }
}