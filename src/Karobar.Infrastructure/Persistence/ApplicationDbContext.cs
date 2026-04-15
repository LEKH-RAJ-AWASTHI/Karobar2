using System.Reflection;
using Karobar.Application.Interfaces;
using Karobar.Domain.Entities;
using Karobar.Infrastructure.Identity;
using Karobar.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Karobar.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor,
        ICurrentUserService currentUserService) 
        : base(options)
    {
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
        _currentUserService = currentUserService;
    }

    public DbSet<Ledger> Ledgers => Set<Ledger>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionLine> TransactionLines => Set<TransactionLine>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanEvent> LoanEvents => Set<LoanEvent>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);

        // Soft-delete and Tenant (ShopId) query filters
        builder.Entity<Ledger>().HasQueryFilter(e => !e.IsDeleted && e.ShopId == _currentUserService.ShopId);
        builder.Entity<Transaction>().HasQueryFilter(e => !e.IsDeleted && e.ShopId == _currentUserService.ShopId);
        builder.Entity<Loan>().HasQueryFilter(e => !e.IsDeleted && e.ShopId == _currentUserService.ShopId);
        builder.Entity<LoanEvent>().HasQueryFilter(e => !e.IsDeleted && e.ShopId == _currentUserService.ShopId);
        builder.Entity<Inventory>().HasQueryFilter(e => !e.IsDeleted && e.ShopId == _currentUserService.ShopId);
        builder.Entity<SystemSetting>().HasQueryFilter(e => !e.IsDeleted && e.ShopId == _currentUserService.ShopId);
        builder.Entity<ApplicationUser>().HasQueryFilter(e => e.ShopId == _currentUserService.ShopId);

        // Decimal precision
        builder.Entity<TransactionLine>(e =>
        {
            e.Property(p => p.Debit).HasPrecision(18, 4);
            e.Property(p => p.Credit).HasPrecision(18, 4);
            e.Property(p => p.Quantity).HasPrecision(18, 4);
        });

        builder.Entity<Loan>(e =>
        {
            e.Property(p => p.PrincipalAmount).HasPrecision(18, 4);
            e.Property(p => p.InterestRate).HasPrecision(8, 4);
        });

        builder.Entity<LoanEvent>(e =>
        {
            e.Property(p => p.Amount).HasPrecision(18, 4);
        });

        builder.Entity<Inventory>(e =>
        {
            e.Property(p => p.QuantityInKg).HasPrecision(18, 4);
        });

        builder.Entity<SystemSetting>(e =>
        {
            e.Property(p => p.InterestRate).HasPrecision(8, 4);
            e.Property(p => p.LabourChargePerKatta).HasPrecision(18, 4);
            e.Property(p => p.KattaToKg).HasPrecision(18, 4);
        });

        // Concurrency token (RowVersion)
        builder.Entity<Ledger>().Property(p => p.RowVersion).IsRowVersion();
        builder.Entity<Transaction>().Property(p => p.RowVersion).IsRowVersion();
        builder.Entity<Loan>().Property(p => p.RowVersion).IsRowVersion();
        builder.Entity<LoanEvent>().Property(p => p.RowVersion).IsRowVersion();
        builder.Entity<Inventory>().Property(p => p.RowVersion).IsRowVersion();
        builder.Entity<SystemSetting>().Property(p => p.RowVersion).IsRowVersion();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction == null)
            await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction != null)
            await Database.CurrentTransaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction != null)
            await Database.CurrentTransaction.RollbackAsync(cancellationToken);
    }
}
