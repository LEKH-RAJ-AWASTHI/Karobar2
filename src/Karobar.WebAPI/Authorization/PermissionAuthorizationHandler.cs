using Microsoft.AspNetCore.Authorization;

namespace Karobar.WebAPI.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User == null)
        {
            return Task.CompletedTask;
        }

        // Get all claims of type "Permission"
        var permissionss = context.User.Claims
            .Where(x => x.Type == "Permission" || x.Type == "permission" || x.Type == "Permissions" || x.Type == "permissions")
            .Select(x => x.Value);

        if (permissionss.Any(p => p == requirement.Permission))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
