namespace Loan.Management.Services.Loans.Dtos;

public class LoanCalculationDto
{
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal MonthlyPayment { get; set; }
    public int TotalMonths { get; set; }
    public decimal DailyInterest { get; set; }
}