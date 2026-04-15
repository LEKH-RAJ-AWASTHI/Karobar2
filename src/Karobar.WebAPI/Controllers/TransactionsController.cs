using Karobar.Application.Features.Transactions.Commands.CreateTransaction;
using Karobar.Application.Features.Transactions.Queries.GetTransactionsByLedger;
using Karobar.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karobar.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Accountant,Cashier")]
    public async Task<ActionResult<Guid>> Create(CreateTransactionCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("by-ledger/{ledgerId:guid}")]
    public async Task<ActionResult<TransactionsByLedgerDto>> GetByLedger(
        Guid ledgerId, [FromQuery] TransactionType? type, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetTransactionsByLedgerQuery(ledgerId, type, page, pageSize));
        return Ok(result);
    }
}
