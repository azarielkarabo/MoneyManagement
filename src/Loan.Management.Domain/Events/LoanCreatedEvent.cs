namespace Loan.Management.Events
{
    public class LoanCreatedEvent
    {
        public Entities.Loan Loan { get; set; }

        public LoanCreatedEvent(Entities.Loan loan)
        {
            Loan = loan;
        }
    }
}
