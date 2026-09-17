using ERP_Government.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace ERP_Government.Application.Common.Behaviours;

public class UnhandledExceptionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehaviour(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogDebug("Request cancelled for {RequestType}", typeof(TRequest).Name);
            throw;
        }
        catch (ERP_Government.Application.Common.Exceptions.NotFoundException)
        {
            throw;
        }
        catch (ERP_Government.Application.Common.Exceptions.ValidationException)
        {
            throw;
        }
        catch (ForbiddenAccessException)
        {
            throw;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogError(ex, "Unhandled exception for request {RequestName} {@Request}", requestName, request);
            throw;
        }
    }
}
