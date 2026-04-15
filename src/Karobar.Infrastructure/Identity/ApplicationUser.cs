using Microsoft.AspNetCore.Identity;

using Karobar.Domain.Common;

namespace Karobar.Infrastructure.Identity;

public class ApplicationUser : IdentityUser, IMustHaveShop
{
    public string FullName { get; set; } = string.Empty;
    public Guid ShopId { get; set; }
}
