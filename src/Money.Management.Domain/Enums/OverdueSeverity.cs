namespace Money.Management.Enums
{
    public enum OverdueSeverity
    {
        None = 0,
        Medium = 1,    // < 30 days
        High = 2,      // 30-59 days
        Critical = 3   // 60+ days
    }
}
