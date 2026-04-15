using Karobar.Application.Interfaces;
using Karobar.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Karobar.Infrastructure.Persistence.Interceptors;

public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditableEntitySaveChangesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = _currentUserService.UserId ?? "System";
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedBy = _currentUserService.UserId ?? "System";
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<IMustHaveShop>())
        {
            if (entry.State == EntityState.Added && entry.Entity.ShopId == Guid.Empty)
            {
                entry.Entity.ShopId = _currentUserService.ShopId;
            }
        }
        
        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
                
                // If it's auditable, update the updated date too
                if (entry.Entity is AuditableEntity auditable)
                {
                    auditable.UpdatedBy = _currentUserService.UserId ?? "System";
                    auditable.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
