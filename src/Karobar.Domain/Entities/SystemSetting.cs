using Karobar.Domain.Common;

namespace Karobar.Domain.Entities;

public class SystemSetting : AuditableEntity
{
    public decimal InterestRate { get; set; } = 2.0m;
    public decimal LabourChargePerKatta { get; set; } = 10.0m;
    public decimal KattaToKg { get; set; } = 50.0m;
}
