using System.ComponentModel.DataAnnotations;

namespace Money.Management.Services.Settings
{
    // Bad-payer rules, in days past a loan's expected payback date.
    public class BadPayerSettingsDto
    {
        // A borrower becomes a bad payer once a loan is more than this many days late.
        [Range(0, 3650)]
        public int GraceDays { get; set; }

        // Days late from which a bad payer is rated High.
        [Range(1, 3650)]
        public int HighDays { get; set; }

        // Days late from which a bad payer is rated Critical.
        [Range(1, 3650)]
        public int CriticalDays { get; set; }
    }
}
