using Karobar.Domain.Entities;

namespace Karobar.Domain.Services;

public class InterestCalculationService
{
    public decimal CalculateInterest(Loan loan, DateTime calculateUntil, decimal defaultInterestRate = 2m)
    {
        var interestRate = loan.InterestRate > 0 ? loan.InterestRate : defaultInterestRate;
        
        var events = loan.Events.OrderBy(e => e.Date).ToList();
        
        decimal balance = loan.PrincipalAmount;
        DateTime lastDate = loan.StartDate;
        decimal totalInterest = 0;
        
        foreach(var evt in events)
        {
            if (evt.Date <= lastDate) continue;
            
            var days = (evt.Date - lastDate).TotalDays;
            var months = (decimal)days / 30m;
            
            months = Math.Round(months * 2m, MidpointRounding.AwayFromZero) / 2m;
            
            totalInterest += (balance * interestRate * months) / 100m;
            
            balance += evt.Amount;
            lastDate = evt.Date;
        }
        
        if (calculateUntil > lastDate)
        {
            var days = (calculateUntil - lastDate).TotalDays;
            var months = (decimal)days / 30m;
            months = Math.Round(months * 2m, MidpointRounding.AwayFromZero) / 2m;
            totalInterest += (balance * interestRate * months) / 100m;
        }
        
        return totalInterest;
    }
}
