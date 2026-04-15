using Karobar.Domain.Common;

namespace Karobar.Domain.Entities;

public class Inventory : AuditableEntity
{
    public Guid LedgerId { get; set; }
    public Ledger Ledger { get; set; } = null!;
    
    public decimal QuantityInKg { get; set; }
}
