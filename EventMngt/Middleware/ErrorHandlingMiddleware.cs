using System.Net;
using System.Text.Json;
using FluentValidation;

namespace EventMngt.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        var response = context.Response;
        response.ContentType = "application/json";
        
        var errorResponse = new
        {
            StatusCode = GetStatusCode(exception),
            Message = GetMessage(exception),
            Details = GetDetails(exception)
        };

        response.StatusCode = (int)errorResponse.StatusCode;
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);
        
        await response.WriteAsync(JsonSerializer.Serialize(errorResponse));
    }

    private static HttpStatusCode GetStatusCode(Exception exception) => exception switch
    {
        ValidationException => HttpStatusCode.BadRequest,
        UnauthorizedAccessException => HttpStatusCode.Unauthorized,
        KeyNotFoundException => HttpStatusCode.NotFound,
        _ => HttpStatusCode.InternalServerError
    };

    private static string GetMessage(Exception exception) => exception switch
    {
        ValidationException => "Validation failed",
        UnauthorizedAccessException => "Unauthorized access",
        KeyNotFoundException => "Resource not found",
        _ => "An unexpected error occurred"
    };

    private static object? GetDetails(Exception exception) => exception switch
    {
        ValidationException validationException => validationException.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            ),
        _ => null
    };
} 