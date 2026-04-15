using MediatR;
using Karobar.Application.Interfaces;

namespace Karobar.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _dbContext;

    public TransactionBehavior(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (typeof(TRequest).Name.EndsWith("Command"))
        {
            await _dbContext.BeginTransactionAsync(cancellationToken);
            try
            {
                var response = await next();
                await _dbContext.CommitTransactionAsync(cancellationToken);
                return response;
            }
            catch
            {
                await _dbContext.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        return await next();
    }
}
