using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Karobar.Application.Interfaces;
using Karobar.Domain.Entities;
using Karobar.Domain.Enums;

namespace Karobar.Application.Features.Ledgers.Commands.CreateLedger;

public class CreateLedgerCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public LedgerType Type { get; set; }
    public decimal OpeningBalance { get; set; }
    public string BalanceType { get; set; } = "Receivable";
    public DateTime? OpeningBalanceDate { get; set; }
}

public class CreateLedgerCommandValidator : AbstractValidator<CreateLedgerCommand>
{
    public CreateLedgerCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(200)
            .NotEmpty();
            
        RuleFor(v => v.Type)
            .IsInEnum();

        RuleFor(v => v.OpeningBalance)
            .GreaterThanOrEqualTo(0);

        RuleFor(v => v.BalanceType)
            .Must(x => x == "Receivable" || x == "Payable")
            .When(x => x.OpeningBalance > 0);
    }
}

public class CreateLedgerCommandHandler : IRequestHandler<CreateLedgerCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateLedgerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateLedgerCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the Ledger entity
        var entity = new Ledger
        {
            Name = request.Name,
            Type = request.Type
        };

        _context.Ledgers.Add(entity);

        // 2. Handle Opening Balance if present
        if (request.OpeningBalance > 0)
        {
            // Ensure "Opening Balance Adjustment" ledger exists for this shop
            var adjLedger = await _context.Ledgers
                .FirstOrDefaultAsync(l => l.Name == "Opening Balance Adjustment" && l.Type == LedgerType.Equity, cancellationToken);
            
            if (adjLedger == null)
            {
                adjLedger = new Ledger { Name = "Opening Balance Adjustment", Type = LedgerType.Equity };
                _context.Ledgers.Add(adjLedger);
            }

            // Create the opening balance transaction
            var transaction = new Transaction
            {
                Date = request.OpeningBalanceDate ?? DateTime.UtcNow,
                Type = TransactionType.Journal,
                Description = $"Opening Balance for {request.Name}",
                ReferenceNo = "OB-" + Guid.NewGuid().ToString("N")[..8].ToUpper(),
                IsFinalized = false // Start as false to allow adding lines
            };

            if (request.BalanceType == "Receivable")
            {
                transaction.AddLine(new TransactionLine { Ledger = entity, Debit = request.OpeningBalance });
                transaction.AddLine(new TransactionLine { Ledger = adjLedger, Credit = request.OpeningBalance });
            }
            else // Payable
            {
                transaction.AddLine(new TransactionLine { Ledger = entity, Credit = request.OpeningBalance });
                transaction.AddLine(new TransactionLine { Ledger = adjLedger, Debit = request.OpeningBalance });
            }

            transaction.IsFinalized = true; // Now finalize it
            _context.Transactions.Add(transaction);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
