using Karobar.Infrastructure.Identity;
using Karobar.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Karobar.Infrastructure.Persistence;

public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        // Default roles and permissions
        var adminPermissions = new[]
        {
            Permissions.Ledgers.View, Permissions.Ledgers.Create, Permissions.Ledgers.Update, Permissions.Ledgers.Delete,
            Permissions.Transactions.View, Permissions.Transactions.Create, Permissions.Transactions.Update, Permissions.Transactions.Delete,
            Permissions.Users.Manage,
            Permissions.Inventory.Manage
        };

        var managerPermissions = new[]
        {
            Permissions.Ledgers.View, Permissions.Ledgers.Create, Permissions.Ledgers.Update,
            Permissions.Transactions.View, Permissions.Transactions.Create,
            Permissions.Inventory.Manage
        };

        var standardPermissions = new[]
        {
            Permissions.Ledgers.View,
            Permissions.Transactions.View, Permissions.Transactions.Create
        };

        var rolesWithPermissions = new Dictionary<string, string[]>
        {
            { "Admin", adminPermissions },
            { "Manager", managerPermissions },
            { "Accountant", managerPermissions },
            { "Cashier", standardPermissions }
        };

        foreach (var roleEntry in rolesWithPermissions)
        {
            var roleName = roleEntry.Key;
            var permissions = roleEntry.Value;

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                role = new IdentityRole(roleName);
                await _roleManager.CreateAsync(role);
            }

            var existingClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var permission in permissions)
            {
                if (!existingClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    await _roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("Permission", permission));
                }
            }
        }

        // Default Admin User
        var adminRole = "Admin";
        var adminUserName = "admin@karobar.local";
        if (_userManager.Users.All(u => u.UserName != adminUserName))
        {
            var adminUser = new ApplicationUser { UserName = adminUserName, Email = adminUserName, FullName = "System Administrator" };
            await _userManager.CreateAsync(adminUser, "Admin123!");
            await _userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
}
