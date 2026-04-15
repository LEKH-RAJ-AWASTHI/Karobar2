using MediatR;
using Karobar.Application.Interfaces;
using Karobar.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Karobar.Application.Features.Transactions.Queries.GetTransactionsByLedger;

public record GetTransactionsByLedgerQuery(Guid LedgerId, TransactionType? Type, int Page = 1, int PageSize = 20) : IRequest<TransactionsByLedgerDto>;

public record TransactionSummaryDto(
    Guid TransactionId,
    DateTime Date,
    TransactionType Type,
    string ReferenceNo,
    string Description,
    decimal Debit,
    decimal Credit,
    bool IsFinalized
);

public record TransactionsByLedgerDto(
    List<TransactionSummaryDto> Transactions,
    int TotalCount,
    int Page,
    int PageSize
);

public class GetTransactionsByLedgerQueryHandler : IRequestHandler<GetTransactionsByLedgerQuery, TransactionsByLedgerDto>
{
    private readonly IApplicationDbContext _context;

    public GetTransactionsByLedgerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TransactionsByLedgerDto> Handle(GetTransactionsByLedgerQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TransactionLines
            .Include(tl => tl.Transaction)
            .Where(tl => tl.LedgerId == request.LedgerId);

        if (request.Type.HasValue)
            query = query.Where(tl => tl.Transaction.Type == request.Type.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var results = await query
            .OrderByDescending(tl => tl.Transaction.Date)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(tl => new TransactionSummaryDto(
                tl.TransactionId,
                tl.Transaction.Date,
                tl.Transaction.Type,
                tl.Transaction.ReferenceNo,
                tl.Transaction.Description,
                tl.Debit,
                tl.Credit,
                tl.Transaction.IsFinalized
            ))
            .ToListAsync(cancellationToken);

        return new TransactionsByLedgerDto(results, totalCount, request.Page, request.PageSize);
    }
}
