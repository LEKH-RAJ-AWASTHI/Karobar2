using Karobar.Application.Features.Loans.Commands.CreateLoan;
using Karobar.Application.Features.Loans.Commands.AddLoanPayment;
using Karobar.Application.Features.Loans.Queries.GetLoanDetails;
using Karobar.Application.Features.Loans.Queries.GetInterestCalculation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karobar.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly IMediator _mediator;

    public LoansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<Guid>> Create(CreateLoanCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("payment")]
    [Authorize(Roles = "Admin,Manager,Cashier")]
    public async Task<ActionResult<Guid>> AddPayment(AddLoanPaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("{loanId:guid}")]
    public async Task<ActionResult<LoanDetailsDto>> GetDetails(Guid loanId)
    {
        var result = await _mediator.Send(new GetLoanDetailsQuery(loanId));
        return Ok(result);
    }

    [HttpGet("{loanId:guid}/interest")]
    public async Task<ActionResult<InterestCalculationDto>> GetInterest(Guid loanId, [FromQuery] DateTime? calculateUntil)
    {
        var result = await _mediator.Send(new GetInterestCalculationQuery(loanId, calculateUntil));
        return Ok(result);
    }
}
