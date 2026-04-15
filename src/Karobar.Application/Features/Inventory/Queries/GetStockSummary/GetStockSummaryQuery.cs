using MediatR;
using Karobar.Application.Interfaces;
using Karobar.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Karobar.Application.Features.Inventory.Queries.GetStockSummary;

public record GetStockSummaryQuery() : IRequest<List<StockItemDto>>;

public record StockItemDto(Guid LedgerId, string ProductName, decimal QuantityInKg);

public class GetStockSummaryQueryHandler : IRequestHandler<GetStockSummaryQuery, List<StockItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStockSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockItemDto>> Handle(GetStockSummaryQuery request, CancellationToken cancellationToken)
    {
        var stocks = await _context.Inventories
            .Include(i => i.Ledger)
            .Where(i => i.Ledger.Type == LedgerType.Product)
            .Select(i => new StockItemDto(i.LedgerId, i.Ledger.Name, i.QuantityInKg))
            .ToListAsync(cancellationToken);

        return stocks;
    }
}
