using Karobar.Domain.Common;

namespace Karobar.Domain.Entities;

public class Loan : AuditableEntity
{
    public Guid LedgerId { get; set; }  // Farmer
    public Ledger Ledger { get; set; } = null!;

    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public DateTime StartDate { get; set; }
    public bool IsClosed { get; set; }

    public ICollection<LoanEvent> Events { get; set; } = [];
}
