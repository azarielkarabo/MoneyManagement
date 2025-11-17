namespace Loan.Management.Services.Dashboards.Dtos
{
    public class PaymentMethodDistributionDto
    {
        public string Method { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }
}