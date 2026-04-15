using FluentValidation;
using MediatR;
using Karobar.Application.Interfaces;
using Karobar.Domain.Entities;
using Karobar.Domain.Enums;

namespace Karobar.Application.Features.Transactions.Commands.CreateTransaction;

public record TransactionLineDto(Guid LedgerId, decimal Debit, decimal Credit, decimal? Quantity);

public record CreateTransactionCommand(
    DateTime Date,
    TransactionType Type,
    string Description,
    string? IdempotencyKey,
    List<TransactionLineDto> Lines
) : IRequest<Guid>;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(v => v.Date).NotEmpty();
        RuleFor(v => v.Type).IsInEnum();
        RuleFor(v => v.Description).NotEmpty().MaximumLength(500);
        RuleFor(v => v.Lines).NotEmpty().WithMessage("At least one transaction line is required.");

        RuleForEach(v => v.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.LedgerId).NotEmpty();
            line.RuleFor(l => l.Debit).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.Credit).GreaterThanOrEqualTo(0);
        });

        RuleFor(v => v.Lines)
            .Must(lines => lines.Sum(l => l.Debit) == lines.Sum(l => l.Credit))
            .WithMessage("Total Debit must equal Total Credit (double-entry rule).");
    }
}

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateTransactionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        // Idempotency check
        if (!string.IsNullOrEmpty(request.IdempotencyKey))
        {
            var existing = _context.Transactions
                .FirstOrDefault(t => t.IdempotencyKey == request.IdempotencyKey);
            if (existing != null)
                return existing.Id;
        }

        // Generate reference number
        var refPrefix = request.Type switch
        {
            TransactionType.Receipt => "RCP",
            TransactionType.Voucher => "VCH",
            TransactionType.Purchase => "PUR",
            TransactionType.Sale => "SAL",
            _ => "TXN"
        };
        var refNo = $"{refPrefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var transaction = new Transaction
        {
            Date = request.Date,
            Type = request.Type,
            Description = request.Description,
            ReferenceNo = refNo,
            IdempotencyKey = request.IdempotencyKey
        };

        foreach (var lineDto in request.Lines)
        {
            transaction.AddLine(new TransactionLine
            {
                LedgerId = lineDto.LedgerId,
                Debit = lineDto.Debit,
                Credit = lineDto.Credit,
                Quantity = lineDto.Quantity
            });
        }

        if (!transaction.IsBalanced)
            throw new InvalidOperationException("Transaction is not balanced. Total Debit must equal Total Credit.");

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        return transaction.Id;
    }
}
