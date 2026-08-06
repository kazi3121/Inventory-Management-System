using System.Net;
using System.Text.Json;
using FluentValidation;

namespace IMS.API.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // Fix: Use 'object' instead of 'var' to allow implicit pattern matching with different anonymous objects
        object response = exception switch
        {
            ValidationException validationEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Validation failed.",
                Errors = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            },
            InvalidOperationException invalidOpEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = invalidOpEx.Message,
                Errors = (object?)null
            },
            UnauthorizedAccessException unauthorizedEx => new
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = unauthorizedEx.Message,
                Errors = (object?)null
            },
            KeyNotFoundException keyNotFoundEx => new
           {
               StatusCode = (int)HttpStatusCode.NotFound,
               Message = keyNotFoundEx.Message,
               Errors = (object?)null
            },
            _ => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An internal server error occurred.",
                Errors = (object?)null
            }
        };

        // Determine status code dynamically from response object
        var statusCodeProperty = response.GetType().GetProperty("StatusCode")?.GetValue(response);
        context.Response.StatusCode = statusCodeProperty is int status ? status : (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}