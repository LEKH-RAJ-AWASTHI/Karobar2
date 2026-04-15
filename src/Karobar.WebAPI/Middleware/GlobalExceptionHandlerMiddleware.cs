using FluentValidation;
using System.Net;
using System.Text.Json;

namespace Karobar.WebAPI.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred.");

        var statusCode = exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Forbidden,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var response = new
        {
            Success = false,
            Error = exception switch
            {
                ValidationException => "Validation failed.",
                UnauthorizedAccessException => "You are not authorized to perform this action.",
                KeyNotFoundException => "The requested resource was not found.",
                InvalidOperationException invEx => invEx.Message,
                _ => "An unexpected error occurred."
            },
            Errors = exception is ValidationException validationEx
                ? validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                : null
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
