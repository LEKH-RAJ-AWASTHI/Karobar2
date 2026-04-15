using FluentValidation;
using MediatR;
using Karobar.Application.Interfaces;

namespace Karobar.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

public record LoginResponse(bool Succeeded, string Token, string Email = "", string[]? Roles = null);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(v => v.Email).NotEmpty().EmailAddress();
        RuleFor(v => v.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, token, email, roles) = await _identityService.LoginAsync(request.Email, request.Password);
        return new LoginResponse(succeeded, token, email, roles);
    }
}
