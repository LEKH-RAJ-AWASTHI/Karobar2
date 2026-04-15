using FluentValidation;
using MediatR;
using Karobar.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Karobar.Application.Features.Ledgers.Commands.DeleteLedger;

public record DeleteLedgerCommand(Guid LedgerId) : IRequest;

public class DeleteLedgerCommandHandler : IRequestHandler<DeleteLedgerCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteLedgerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteLedgerCommand request, CancellationToken cancellationToken)
    {
        var ledger = await _context.Ledgers
            .Include(l => l.TransactionLines) // Load transaction lines to check constrain
            .FirstOrDefaultAsync(l => l.Id == request.LedgerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Ledger {request.LedgerId} not found.");

        if (ledger.TransactionLines.Any())
        {
            throw new InvalidOperationException("Cannot delete ledger because it has associated transactions.");
        }

        _context.Ledgers.Remove(ledger);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
