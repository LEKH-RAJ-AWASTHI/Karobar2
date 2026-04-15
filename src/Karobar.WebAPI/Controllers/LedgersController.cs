using Karobar.Application.Features.Ledgers.Commands.CreateLedger;
using Karobar.Application.Features.Ledgers.Queries.GetLedgerBalance;
using Karobar.Application.Features.Ledgers.Queries.GetLedgerStatement;
using Karobar.Application.Features.Ledgers.Queries.GetLedgers;
using Karobar.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karobar.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LedgersController : ControllerBase
{
    private readonly IMediator _mediator;

    public LedgersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>GET /api/Ledgers — list all ledgers with optional search, type filter, pagination</summary>
    [HttpGet]
    public async Task<ActionResult<LedgersListDto>> GetAll(
        [FromQuery] string? search = null,
        [FromQuery] LedgerType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetLedgersQuery(search, type, page, pageSize));
        return Ok(result);
    }

    /// <summary>POST /api/Ledgers — create a new ledger</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateLedgerCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>GET /api/Ledgers/{ledgerId}/balance</summary>
    [HttpGet("{ledgerId:guid}/balance")]
    public async Task<ActionResult<LedgerBalanceDto>> GetBalance(Guid ledgerId)
    {
        var result = await _mediator.Send(new GetLedgerBalanceQuery(ledgerId));
        return Ok(result);
    }

    /// <summary>GET /api/Ledgers/{ledgerId}/statement</summary>
    [HttpGet("{ledgerId:guid}/statement")]
    public async Task<ActionResult<LedgerStatementDto>> GetStatement(
        Guid ledgerId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetLedgerStatementQuery(ledgerId, fromDate, toDate, page, pageSize));
        return Ok(result);
    }

    /// <summary>DELETE /api/Ledgers/{ledgerId} — admin only</summary>
    [HttpDelete("{ledgerId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid ledgerId)
    {
        await _mediator.Send(new Karobar.Application.Features.Ledgers.Commands.DeleteLedger.DeleteLedgerCommand(ledgerId));
        return NoContent();
    }
}
