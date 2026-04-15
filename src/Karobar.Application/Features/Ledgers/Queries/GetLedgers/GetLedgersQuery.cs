using MediatR;
using Karobar.Application.Interfaces;
using Karobar.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Karobar.Application.Features.Ledgers.Queries.GetLedgers;

public record GetLedgersQuery(
    string? Search = null,
    LedgerType? Type = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<LedgersListDto>;

public record LedgerSummaryDto(
    Guid Id,
    string Name,
    LedgerType Type,
    bool IsActive,
    decimal Balance
);

public record LedgersListDto(
    List<LedgerSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

public class GetLedgersQueryHandler : IRequestHandler<GetLedgersQuery, LedgersListDto>
{
    private readonly IApplicationDbContext _context;

    public GetLedgersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LedgersListDto> Handle(GetLedgersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Ledgers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(l => l.Name.Contains(request.Search));

        if (request.Type.HasValue)
            query = query.Where(l => l.Type == request.Type.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(l => l.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(l => new LedgerSummaryDto(
                l.Id, 
                l.Name, 
                l.Type, 
                l.IsActive,
                _context.TransactionLines
                    .Where(tl => tl.LedgerId == l.Id)
                    .Sum(tl => tl.Debit - tl.Credit)
            ))
            .ToListAsync(cancellationToken);

        return new LedgersListDto(items, totalCount, request.Page, request.PageSize);
    }
}
