using System.Security.Claims;
using Karobar.Application.Interfaces;

namespace Karobar.WebAPI.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public Guid ShopId
    {
        get
        {
            var shopIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("ShopId")?.Value;
            return Guid.TryParse(shopIdClaim, out var shopId) ? shopId : Guid.Empty;
        }
    }

    public bool HasPermission(string permission)
    {
        // Simple permission check placeholder. Roles/Claims logic here.
        return true; 
    }
}
