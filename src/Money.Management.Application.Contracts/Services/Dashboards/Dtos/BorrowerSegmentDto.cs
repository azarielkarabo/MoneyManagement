namespace Money.Management.Services.Dashboards.Dtos
{
    public class BorrowerSegmentDto
    {
        public string SegmentName { get; set; }
        public int Count { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal Percentage { get; set; }
    }
}