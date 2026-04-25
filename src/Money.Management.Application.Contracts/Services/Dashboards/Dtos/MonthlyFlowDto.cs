namespace Money.Management.Services.Dashboards.Dtos
{
    public class MonthlyFlowDto
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public decimal LentAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public decimal NetFlow { get; set; }
        public int MoneysCount { get; set; }
        public int PaymentsCount { get; set; }
    }
}