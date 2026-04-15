namespace Karobar.Application.Interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, string Token, string Email, string[] Roles)> LoginAsync(string email, string password);
    Task<(bool Succeeded, string UserId)> RegisterAsync(string email, string password, string fullName, string role);
    Task<bool> IsInRoleAsync(string userId, string role);
    Task<IList<string>> GetUserRolesAsync(string userId);
}
