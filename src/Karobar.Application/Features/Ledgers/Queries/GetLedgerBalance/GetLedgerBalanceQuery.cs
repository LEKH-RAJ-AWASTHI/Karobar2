using MediatR;
using Karobar.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Karobar.Application.Features.Ledgers.Queries.GetLedgerBalance;

public record GetLedgerBalanceQuery(Guid LedgerId) : IRequest<LedgerBalanceDto>;

public record LedgerBalanceDto(Guid LedgerId, string LedgerName, decimal TotalDebit, decimal TotalCredit, decimal Balance);

public class GetLedgerBalanceQueryHandler : IRequestHandler<GetLedgerBalanceQuery, LedgerBalanceDto>
{
    private readonly IApplicationDbContext _context;

    public GetLedgerBalanceQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LedgerBalanceDto> Handle(GetLedgerBalanceQuery request, CancellationToken cancellationToken)
    {
        var ledger = await _context.Ledgers
            .FirstOrDefaultAsync(l => l.Id == request.LedgerId, cancellationToken)
            ?? throw new InvalidOperationException($"Ledger {request.LedgerId} not found.");

        var totalDebit = await _context.TransactionLines
            .Where(tl => tl.LedgerId == request.LedgerId)
            .SumAsync(tl => tl.Debit, cancellationToken);

        var totalCredit = await _context.TransactionLines
            .Where(tl => tl.LedgerId == request.LedgerId)
            .SumAsync(tl => tl.Credit, cancellationToken);

        return new LedgerBalanceDto(
            ledger.Id,
            ledger.Name,
            totalDebit,
            totalCredit,
            totalDebit - totalCredit
        );
    }
}
