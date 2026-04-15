using FluentValidation;
using MediatR;
using Karobar.Application.Interfaces;
using Karobar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Karobar.Application.Features.Loans.Commands.AddLoanPayment;

public record AddLoanPaymentCommand(
    Guid LoanId,
    decimal Amount,
    DateTime Date
) : IRequest<Guid>;

public class AddLoanPaymentCommandValidator : AbstractValidator<AddLoanPaymentCommand>
{
    public AddLoanPaymentCommandValidator()
    {
        RuleFor(v => v.LoanId).NotEmpty();
        RuleFor(v => v.Amount).GreaterThan(0);
        RuleFor(v => v.Date).NotEmpty();
    }
}

public class AddLoanPaymentCommandHandler : IRequestHandler<AddLoanPaymentCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddLoanPaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddLoanPaymentCommand request, CancellationToken cancellationToken)
    {
        var loan = await _context.Loans
            .Include(l => l.Events)
            .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken)
            ?? throw new InvalidOperationException($"Loan {request.LoanId} not found.");

        if (loan.IsClosed)
            throw new InvalidOperationException("Cannot add payment to a closed loan.");

        // Negative amount = repayment
        var loanEvent = new LoanEvent
        {
            LoanId = request.LoanId,
            Date = request.Date,
            Amount = -request.Amount
        };

        _context.LoanEvents.Add(loanEvent);
        await _context.SaveChangesAsync(cancellationToken);

        return loanEvent.Id;
    }
}
