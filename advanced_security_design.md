# Karobar Advanced RBAC, Multi-Tenancy & Audit Design 

This document details the architectural decisions and step-by-step strategy for migrating the current single-tenant, simple-role monolithic application to a fully secured, multi-tenant (Shop-based), and fully auditable secure system.

## 1. Multi-Tenant Shop Isolation Design

### Approach
We will implement "Logical Separation" multi-tenancy in the database using a **ShopId** discriminator column.

1. **`IMustHaveShop` Interface**: We will create an interface imposing `Guid ShopId { get; set; }`.
2. **Entity Updates**: `Ledger`, `Transaction`, `Loan`, `Inventory`, and `ApplicationUser` will inherit this interface.
3. **CurrentUserService**: Will be updated to securely extract `ShopId` from the JWT token claims.
4. **EF Core Integration**:
   - **Reads**: We will inject the `ICurrentUserService` into the `ApplicationDbContext` and add `HasQueryFilter(e => e.ShopId == _currentUserService.ShopId)` into `OnModelCreating`. This ensures that even if developers forget to check the shop, the DB simply won't return data belonging to another shop.
   - **Writes**: Our `SaveChangesInterceptor` will automatically set the `ShopId` on all incoming `IMustHaveShop` entities to guarantee users cannot create records outside of their tenant.

## 2. Advanced Role-Based & Policy Permissions

### Approach
Instead of hardcoding roles in controllers, we will implement **Claims-Based Permission Authorization**.

1. **Constants**: Create a `Permissions` static class holding constant string keys (e.g., `Permissions.Ledgers.Create = "ledger.create"`).
2. **Mappings as Claims**: We will leverage ASP.NET Core Identity. Permissions will be stored in the database as `AspNetRoleClaims` linked to a given `RoleId`. 
   - `Admin Role` maps to -> `ledger.create`, `ledger.delete`, `users.manage`, etc.
   - `Staff Role` maps to -> `ledger.view`, `inventory.manage`
3. **Custom Authorization Handler**: We will write a custom `PermissionAuthorizationHandler` that intercepts `[Authorize(Policy = "ledger.create")]`. It will check if the user's JWT (or database claims) contains the required permission string.
4. **JWT Adjustments**: Include the `ShopId`, `Role`, and optionally standard permissions directly in the JWT payload to prevent extra database trips.

## 3. Advanced Audit Logging

### Approach
We will build a full property-level change tracking system.

1. **`AuditLog` Entity**: A new infrastructure entity with `UserId`, `Action` (Create/Update/Delete), `TableName`, `Timestamp`, `OldValues` (JSON), and `NewValues` (JSON).
2. **Interceptor Logic**: We will expand our existing `AuditableEntitySaveChangesInterceptor`. Right before `SaveChanges`, we will examine the EF `ChangeTracker`. For every modified entity, we will iterate over its properties, identify what changed, serialize the before/after states of just the changed columns, and insert an `AuditLog` row into the database within the exact same transaction.

## 4. Specific Business Safety Rules

### Approach
The rules regarding "Staff cannot delete" and "Staff can only edit same-day entries" will be enforced globally to guarantee data safety.

1. **Rule Enforcement**: We will add this specialized logic into our `TransactionBehavior` or a specialized MediatR `SecurityBehavior`, or even right inside the specific WebAPI controllers (`Delete`/`Update` endpoints). 
2. **Check**: If `CurrentUserService.Role == "Staff"` and the entity's `CreatedAt.Date != DateTime.UtcNow.Date`, we halt and throw a `ForbiddenException`.

## Step-by-Step Implementation Sequence

If you approve of this design, I will process the implementation in the following distinct phases utilizing our tool capabilities:

*   **Phase 1: Multi-Tenancy Engine**
    *   Update Entities with `ShopId`.
    *   Inject `ShopId` globally into DbContext query filters.
*   **Phase 2: Claims-Based Permissions RBAC**
    *   Implement static permissions, Role-to-Claim seed data initialization.
    *   Setup the custom ASP.NET Core Policy Handlers.
*   **Phase 3: The Audit Trail Module**
    *   Add `AuditLog` model and modify the DB Interceptor to track deep changes.
*   **Phase 4: Enforcement & Refactoring**
    *   Update controllers and Token Service to issue multi-tenant tokens and apply the `[Authorize(Policy="...")]` constraints.
    *   Inject the localized specific data rules (e.g., Staff same-day editing).
