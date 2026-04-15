using Karobar.Application.Features.Inventory.Queries.GetStockSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karobar.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("stock-summary")]
    public async Task<ActionResult<List<StockItemDto>>> GetStockSummary()
    {
        var result = await _mediator.Send(new GetStockSummaryQuery());
        return Ok(result);
    }
}
