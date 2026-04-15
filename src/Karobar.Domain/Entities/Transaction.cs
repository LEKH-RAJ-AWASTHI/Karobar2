using Karobar.Domain.Common;
using Karobar.Domain.Enums;

namespace Karobar.Domain.Entities;

public class Transaction : AuditableEntity
{
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public required string Description { get; set; }
    public required string ReferenceNo { get; set; }
    
    public bool IsFinalized { get; set; }
    public string? IdempotencyKey { get; set; }

    public ICollection<TransactionLine> Lines { get; set; } = [];

    public bool IsBalanced => Lines.Sum(l => l.Debit) == Lines.Sum(l => l.Credit);

    public void AddLine(TransactionLine line)
    {
        if (IsFinalized)
            throw new InvalidOperationException("Cannot add lines to a finalized transaction.");
            
        Lines.Add(line);
    }
}
