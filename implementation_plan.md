# Karobar Grain Trading Backend - Implementation Plan

This document outlines the architecture, layers, and steps to build the .NET Web API backend for the grain trading shop system, strictly adhering to Clean Architecture principles, CQRS (via MediatR), and all domain requirements provided (Ledger, Transactions, Loans, Inventory, and Auth).

## User Review Required

> [!IMPORTANT]
> **.NET Version**
> I will use the `.NET 8.0` (or the latest SDK installed on your machine) for this scaffold. Please confirm if you have a specific SDK requirement (e.g., .NET 9.0). 
>
> **Database Provider**
> The prompt mentions "SQL Server (or PostgreSQL)". I will default to **PostgreSQL (Npgsql)** as it allows cross-platform local development easily. If you prefer SQL Server, please let me know and I will use the `Microsoft.EntityFrameworkCore.SqlServer` provider instead.

## Proposed Architecture

We will structure the monolithic application into 4 distinct projects within a `/src` directory, and add a `/tests` directory for tests.

### 1. Karobar.Domain
Contains core enterprise logic and entities. No dependencies on other layers.
- **Common**: `BaseEntity`, `AuditableEntity` (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, DeletedAt, RowVersion), `ISoftDelete`.
- **Entities**: 
  - `Ledger` (Farmer, Bank, Party, Product, Expense, Interest)
  - `Transaction` (Receipt, Voucher, Purchase, Sale) & `TransactionLine`
  - `Loan` & `LoanEvent`
  - `Inventory` (Stock Tracking)
  - `SystemSetting`
- **Enums**: `LedgerType`, `TransactionType`
- **Domain Services**: `InterestCalculationService`, `InventoryService`

### 2. Karobar.Application
Contains business use cases (CQRS) and interfaces. Depends ONLY on Domain.
- **Commands**: Handlers for `CreateLedger`, `CreateTransaction` (Receipt, Voucher, Purchase, Sale), `CreateLoan`, `AddLoanPayment`.
- **Queries**: Handlers for `GetLedgerStatement`, `GetLedgerBalance`, `GetTransactionsByLedger`, `GetStockSummary`, `GetLoanDetails`, `GetInterestCalculation`.
- **Behaviors**: 
  - `ValidationBehavior` (FluentValidation)
  - `TransactionBehavior` (Wraps handlers in database transactions)
  - `IdempotencyBehavior` (Prevents duplicate requests using `IdempotencyKey`)
- **Interfaces**: `IApplicationDbContext`, `ICurrentUserService`, `IIdentityService`.
- **DTOs**: Reponses and inputs.

### 3. Karobar.Infrastructure
Contains external concerns (DB, Auth, File System). Depends on Application.
- **Persistence**: `ApplicationDbContext`, EF Core Configurations, Interceptors for Auditing / Soft Delete.
- **Identity**: Identity Models (`ApplicationUser`, `ApplicationRole`, `ApplicationRoleClaim`), JWT generation, Database Permission Storage.
- **Services**: Current Time Service, implementations of Domain/Application interfaces.
- **Migrations**: Folder created during DB init.

### 4. Karobar.WebAPI
Presentation layer. Depends on Application and Infrastructure.
- **Controllers**: Segmented by modules (`LedgersController`, `TransactionsController`, `LoansController`, `InventoryController`, `AuthController`).
- **Middleware**: Global Exception Handler.
- **Configurations**: Swagger with JWT Support, Dependency Injection setup, CORS.
- **Seed Data**: Background service to seed minimal Roles and Admin permissions on startup.

## Proposed Changes (Files/Projects)

### Solution Setup
#### [NEW] `Karobar.sln`
#### [NEW] `src/Karobar.Domain/Karobar.Domain.csproj`
#### [NEW] `src/Karobar.Application/Karobar.Application.csproj`
#### [NEW] `src/Karobar.Infrastructure/Karobar.Infrastructure.csproj`
#### [NEW] `src/Karobar.WebAPI/Karobar.WebAPI.csproj`

### Specific Important Implementation Details

- **Double-Entry Validation**: Handled within the `Transaction` entity methods and `CreateTransactionCommand` verification (`Total Debit == Total Credit`).
- **Audit & Soft Delete**: Handled globally in an EF Core `SaveChangesInterceptor`.
- **Transaction Atomicity**: A MediatR pipeline behavior `IPipelineBehavior<TRequest, TResponse>` will open a database transaction before the handler and commit/rollback after.
- **Reference Numbers**: Auto-generated uniquely upon creation if not provided, possibly using a sequence or standard patterned string.
- **Locking/Finalization**: Property `IsFinalized`, enforced through Application layer checks validating that any mutation rejects finalized transactions.
- **Concurrency Handling**: EF Core `[Timestamp]` applied to `RowVersion` and handled on save to throw Domain exceptions.

## Verification Plan

### Automated Steps
1. The scaffold will include compiling the entire solution via `dotnet build`.
2. Ensure EF Core configuration is valid by running `dotnet ef migrations add InitialCreate --project src/Karobar.Infrastructure --startup-project src/Karobar.WebAPI`.
3. Create unit test skeletons in test projects to ensure dependency linkages are properly isolated.

### Manual Verification
1. Run `dotnet run --project src/Karobar.WebAPI`.
2. Ask the user (you) to test the Swagger UI locally to ensure the Seed Data (Roles, Users, Settings) works and that basic JWT tokens can be retrieved.
3. Call a `CreateTransaction` endpoint testing idempotency, atomicity, and Validation.
