using MediatR;
using Karobar.Application.Interfaces;
using Karobar.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Karobar.Application.Features.Loans.Queries.GetLoanDetails;

public record GetLoanDetailsQuery(Guid LoanId) : IRequest<LoanDetailsDto>;

public record LoanEventDto(Guid Id, DateTime Date, decimal Amount);

public record LoanDetailsDto(
    Guid LoanId,
    Guid LedgerId,
    string FarmerName,
    decimal PrincipalAmount,
    decimal InterestRate,
    DateTime StartDate,
    bool IsClosed,
    decimal OutstandingBalance,
    decimal AccruedInterest,
    List<LoanEventDto> Events
);

public class GetLoanDetailsQueryHandler : IRequestHandler<GetLoanDetailsQuery, LoanDetailsDto>
{
    private readonly IApplicationDbContext _context;

    public GetLoanDetailsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LoanDetailsDto> Handle(GetLoanDetailsQuery request, CancellationToken cancellationToken)
    {
        var loan = await _context.Loans
            .Include(l => l.Events)
            .Include(l => l.Ledger)
            .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken)
            ?? throw new InvalidOperationException($"Loan {request.LoanId} not found.");

        var interestService = new InterestCalculationService();
        var accruedInterest = interestService.CalculateInterest(loan, DateTime.UtcNow);

        var outstandingBalance = loan.PrincipalAmount + loan.Events.Sum(e => e.Amount);

        var events = loan.Events
            .OrderBy(e => e.Date)
            .Select(e => new LoanEventDto(e.Id, e.Date, e.Amount))
            .ToList();

        return new LoanDetailsDto(
            loan.Id,
            loan.LedgerId,
            loan.Ledger.Name,
            loan.PrincipalAmount,
            loan.InterestRate,
            loan.StartDate,
            loan.IsClosed,
            outstandingBalance,
            accruedInterest,
            events
        );
    }
}
