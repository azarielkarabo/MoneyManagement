using System.Collections.Generic;

namespace Money.Management.Services.Dashboards.Dtos
{
    /// <summary>
    /// Collection metrics and performance
    /// </summary>
    public class CollectionMetricsDto
    {
        public decimal CollectionEfficiency { get; set; }
        public decimal OnTimePaymentRate { get; set; }
        public decimal LatePaymentRate { get; set; }

        public int PaymentsThisMonth { get; set; }
        public decimal AmountCollectedThisMonth { get; set; }
        public decimal TargetCollectionThisMonth { get; set; }
        public decimal CollectionTargetProgress { get; set; }

        public List<PaymentMethodDistributionDto> PaymentMethodDistribution { get; set; }

        public CollectionMetricsDto()
        {
            PaymentMethodDistribution = new List<PaymentMethodDistributionDto>();
        }
    }
}