using Microsoft.EntityFrameworkCore;
using Karobar.Domain.Entities;

namespace Karobar.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Ledger> Ledgers { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<TransactionLine> TransactionLines { get; }
    DbSet<Loan> Loans { get; }
    DbSet<LoanEvent> LoanEvents { get; }
    DbSet<Inventory> Inventories { get; }
    DbSet<SystemSetting> SystemSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
