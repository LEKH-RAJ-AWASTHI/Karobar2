using MediatR;

namespace Karobar.Application.Behaviors;

public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Idempotency stub implementation.
        // Needs a cache or storage mechanism to properly implement (e.g. tracking key hashes).
        return await next();
    }
}
