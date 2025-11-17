using System.Collections.Generic;

namespace Loan.Management.Services.Dashboards.Dtos
{
    public class MoneyFlowDto
    {
        public List<MonthlyFlowDto> MonthlyData { get; set; }

        public MoneyFlowDto()
        {
            MonthlyData = new List<MonthlyFlowDto>();
        }
    }

    public class MonthlyFlowDto
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public decimal LentAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal NetFlow { get; set; }
    }
}
