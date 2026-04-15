namespace Karobar.Domain.Common;

public abstract class AuditableEntity : BaseEntity, ISoftDelete, IMustHaveShop
{
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public byte[] RowVersion { get; set; } = [];
    
    public Guid ShopId { get; set; }
}
