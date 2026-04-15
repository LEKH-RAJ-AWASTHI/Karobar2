using FluentValidation;
using MediatR;
using Karobar.Application.Interfaces;
using Karobar.Domain.Entities;

namespace Karobar.Application.Features.Loans.Commands.CreateLoan;

public record CreateLoanCommand(
    Guid LedgerId,
    decimal PrincipalAmount,
    decimal InterestRate,
    DateTime StartDate
) : IRequest<Guid>;

public class CreateLoanCommandValidator : AbstractValidator<CreateLoanCommand>
{
    public CreateLoanCommandValidator()
    {
        RuleFor(v => v.LedgerId).NotEmpty();
        RuleFor(v => v.PrincipalAmount).GreaterThan(0);
        RuleFor(v => v.InterestRate).GreaterThanOrEqualTo(0);
        RuleFor(v => v.StartDate).NotEmpty();
    }
}

public class CreateLoanCommandHandler : IRequestHandler<CreateLoanCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateLoanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = new Loan
        {
            LedgerId = request.LedgerId,
            PrincipalAmount = request.PrincipalAmount,
            InterestRate = request.InterestRate,
            StartDate = request.StartDate
        };

        // Add initial loan event (money given to farmer)
        loan.Events.Add(new LoanEvent
        {
            Date = request.StartDate,
            Amount = request.PrincipalAmount
        });

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync(cancellationToken);

        return loan.Id;
    }
}
