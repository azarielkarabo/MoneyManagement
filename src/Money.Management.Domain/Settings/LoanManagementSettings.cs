namespace Money.Management.Settings;

public static class MoneyManagementSettings
{
    private const string Prefix = "MoneyManagement";

    // Bad-payer rules, all counted in days past a loan's expected payback date.
    public static class BadPayers
    {
        private const string BadPayersPrefix = Prefix + ".BadPayers";

        // A borrower becomes a bad payer once a loan is more than this many days late.
        public const string GraceDays = BadPayersPrefix + ".GraceDays";
        public const int DefaultGraceDays = 14;

        // Days late from which a bad payer is rated High / Critical.
        public const string HighDays = BadPayersPrefix + ".HighDays";
        public const int DefaultHighDays = 30;

        public const string CriticalDays = BadPayersPrefix + ".CriticalDays";
        public const int DefaultCriticalDays = 60;
    }
}
