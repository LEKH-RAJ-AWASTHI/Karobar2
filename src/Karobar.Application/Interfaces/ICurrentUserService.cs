namespace Karobar.Application.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    Guid ShopId { get; }
    bool HasPermission(string permission);
}
