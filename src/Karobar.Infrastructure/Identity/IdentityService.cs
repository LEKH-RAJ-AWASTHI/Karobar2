using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Karobar.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Karobar.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<(bool Succeeded, string Token, string Email, string[] Roles)> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return (false, "Invalid credentials.", string.Empty, Array.Empty<string>());

        var isValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isValid)
            return (false, "Invalid credentials.", string.Empty, Array.Empty<string>());

        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);
        return (true, token, user.Email!, roles.ToArray());
    }

    public async Task<(bool Succeeded, string UserId)> RegisterAsync(string email, string password, string fullName, string role)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        await _userManager.AddToRoleAsync(user, role);
        return (true, user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return new List<string>();
        return await _userManager.GetRolesAsync(user);
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Secret"]!));

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new("ShopId", user.ShopId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            
            var identityRole = await _roleManager.FindByNameAsync(role);
            if (identityRole != null)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(identityRole);
                foreach (var rc in roleClaims.Where(c => c.Type == "Permission"))
                {
                    claims.Add(new Claim("Permission", rc.Value));
                }
            }
        }

        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
