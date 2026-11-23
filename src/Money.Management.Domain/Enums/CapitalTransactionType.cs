namespace Money.Management.Enums
{
    public enum CapitalTransactionType
    {
        /// <summary>The balance the lending business started with.</summary>
        OpeningBalance = 0,

        /// <summary>Extra money the lender put into the business.</summary>
        Deposit = 1,

        /// <summary>Money the lender took out of the business.</summary>
        Withdrawal = 2,
    }
}
