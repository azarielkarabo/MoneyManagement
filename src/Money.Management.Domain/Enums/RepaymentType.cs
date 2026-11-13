namespace Money.Management.Enums
{
    /// <summary>
    /// How a loan is repaid. MonthEnd loans fall due at the end of the month they are registered in;
    /// LongTerm loans fall due after the number of months in the term.
    /// </summary>
    public enum RepaymentType
    {
        LongTerm = 0,
        MonthEnd = 1
    }
}
