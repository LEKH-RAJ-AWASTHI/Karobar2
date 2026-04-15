using MediatR;
using Karobar.Application.Interfaces;
using Karobar.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Karobar.Application.Features.Loans.Queries.GetInterestCalculation;

public record GetInterestCalculationQuery(Guid LoanId, DateTime? CalculateUntil) : IRequest<InterestCalculationDto>;

public record InterestCalculationDto(
    Guid LoanId,
    decimal PrincipalAmount,
    decimal InterestRate,
    DateTime StartDate,
    DateTime CalculatedUntil,
    decimal AccruedInterest,
    decimal OutstandingBalance,
    decimal TotalOwed
);

public class GetInterestCalculationQueryHandler : IRequestHandler<GetInterestCalculationQuery, InterestCalculationDto>
{
    private readonly IApplicationDbContext _context;

    public GetInterestCalculationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InterestCalculationDto> Handle(GetInterestCalculationQuery request, CancellationToken cancellationToken)
    {
        var loan = await _context.Loans
            .Include(l => l.Events)
            .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken)
            ?? throw new InvalidOperationException($"Loan {request.LoanId} not found.");

        var calculateUntil = request.CalculateUntil ?? DateTime.UtcNow;

        var interestService = new InterestCalculationService();
        var accruedInterest = interestService.CalculateInterest(loan, calculateUntil);

        var outstandingBalance = loan.PrincipalAmount + loan.Events.Sum(e => e.Amount);
        var totalOwed = outstandingBalance + accruedInterest;

        return new InterestCalculationDto(
            loan.Id,
            loan.PrincipalAmount,
            loan.InterestRate,
            loan.StartDate,
            calculateUntil,
            accruedInterest,
            outstandingBalance,
            totalOwed
        );
    }
}
