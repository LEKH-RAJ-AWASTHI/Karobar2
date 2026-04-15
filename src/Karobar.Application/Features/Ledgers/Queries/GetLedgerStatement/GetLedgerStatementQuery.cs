using MediatR;
using Karobar.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Karobar.Application.Features.Ledgers.Queries.GetLedgerStatement;

public record GetLedgerStatementQuery(Guid LedgerId, DateTime? FromDate, DateTime? ToDate, int Page = 1, int PageSize = 20) : IRequest<LedgerStatementDto>;

public record StatementLineDto(
    DateTime Date,
    string ReferenceNo,
    string Description,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance
);

public record LedgerStatementDto(
    Guid LedgerId,
    string LedgerName,
    decimal OpeningBalance,
    decimal ClosingBalance,
    List<StatementLineDto> Lines,
    int TotalCount,
    int Page,
    int PageSize
);

public class GetLedgerStatementQueryHandler : IRequestHandler<GetLedgerStatementQuery, LedgerStatementDto>
{
    private readonly IApplicationDbContext _context;

    public GetLedgerStatementQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LedgerStatementDto> Handle(GetLedgerStatementQuery request, CancellationToken cancellationToken)
    {
        var ledger = await _context.Ledgers
            .FirstOrDefaultAsync(l => l.Id == request.LedgerId, cancellationToken)
            ?? throw new InvalidOperationException($"Ledger {request.LedgerId} not found.");

        var query = _context.TransactionLines
            .Include(tl => tl.Transaction)
            .Where(tl => tl.LedgerId == request.LedgerId);

        if (request.FromDate.HasValue)
            query = query.Where(tl => tl.Transaction.Date >= request.FromDate.Value);
        if (request.ToDate.HasValue)
            query = query.Where(tl => tl.Transaction.Date <= request.ToDate.Value);

        // Calculate opening balance (all entries before FromDate)
        decimal openingBalance = 0;
        if (request.FromDate.HasValue)
        {
            var priorDebit = await _context.TransactionLines
                .Where(tl => tl.LedgerId == request.LedgerId && tl.Transaction.Date < request.FromDate.Value)
                .SumAsync(tl => tl.Debit, cancellationToken);
            var priorCredit = await _context.TransactionLines
                .Where(tl => tl.LedgerId == request.LedgerId && tl.Transaction.Date < request.FromDate.Value)
                .SumAsync(tl => tl.Credit, cancellationToken);
            openingBalance = priorDebit - priorCredit;
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var lines = await query
            .OrderBy(tl => tl.Transaction.Date)
            .ThenBy(tl => tl.Transaction.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(tl => new
            {
                tl.Transaction.Date,
                tl.Transaction.ReferenceNo,
                tl.Transaction.Description,
                tl.Debit,
                tl.Credit
            })
            .ToListAsync(cancellationToken);

        // Build running balance
        var runningBalance = openingBalance;
        var statementLines = lines.Select(l =>
        {
            runningBalance += l.Debit - l.Credit;
            return new StatementLineDto(l.Date, l.ReferenceNo, l.Description, l.Debit, l.Credit, runningBalance);
        }).ToList();

        return new LedgerStatementDto(
            ledger.Id,
            ledger.Name,
            openingBalance,
            runningBalance,
            statementLines,
            totalCount,
            request.Page,
            request.PageSize
        );
    }
}
