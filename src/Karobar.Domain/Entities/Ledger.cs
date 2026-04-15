using Karobar.Domain.Common;
using Karobar.Domain.Enums;

namespace Karobar.Domain.Entities;

public class Ledger : AuditableEntity
{
    public required string Name { get; set; }
    public LedgerType Type { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public ICollection<TransactionLine> TransactionLines { get; set; } = [];
    public ICollection<Loan> Loans { get; set; } = [];
}
