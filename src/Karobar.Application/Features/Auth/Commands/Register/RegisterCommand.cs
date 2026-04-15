using FluentValidation;
using MediatR;
using Karobar.Application.Interfaces;

namespace Karobar.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password, string FullName, string Role) : IRequest<RegisterResponse>;

public record RegisterResponse(bool Succeeded, string Result);

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(v => v.Email).NotEmpty().EmailAddress();
        RuleFor(v => v.Password).NotEmpty().MinimumLength(6);
        RuleFor(v => v.FullName).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Role).NotEmpty();
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, result) = await _identityService.RegisterAsync(
            request.Email, request.Password, request.FullName, request.Role);
        return new RegisterResponse(succeeded, result);
    }
}
