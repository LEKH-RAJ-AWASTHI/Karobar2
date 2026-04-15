using Karobar.Domain.Common;

namespace Karobar.Domain.Entities;

public class TransactionLine : BaseEntity
{
    public Guid TransactionId { get; set; }
    public Transaction Transaction { get; set; } = null!;

    public Guid LedgerId { get; set; }
    public Ledger Ledger { get; set; } = null!;

    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    
    // E.g., for grains
    public decimal? Quantity { get; set; }
}
