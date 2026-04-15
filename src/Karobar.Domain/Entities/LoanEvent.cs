using Karobar.Domain.Common;

namespace Karobar.Domain.Entities;

public class LoanEvent : AuditableEntity
{
    public Guid LoanId { get; set; }
    public Loan Loan { get; set; } = null!;

    public DateTime Date { get; set; }
    
    // Positive = loan to farmer, Negative = repayment
    public decimal Amount { get; set; }
}
