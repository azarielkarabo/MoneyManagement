namespace Money.Management.Services.Reports.Dtos
{
    public class MonthlyTrendDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthLabel { get; set; }
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }
}
