using Karobar.Application.Features.Auth.Commands.Login;
using Karobar.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karobar.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return Unauthorized(new { result.Token }); // Token contains error message
        return Ok(result);
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
            return BadRequest(new { result.Result });
        return Ok(result);
    }
}
