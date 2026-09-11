using OrderFlow.Domain.Exceptions;
using System.Net;

namespace OrderFlow.Api.Middleware;

public class AppExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AppExceptionHandlerMiddleware> _logger;

    public AppExceptionHandlerMiddleware(RequestDelegate next, ILogger<AppExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error: {Message}", ex.Message);

            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                ErrorMessage = ex.Message
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = GetStatusCodeForException(ex);
            await context.Response.WriteAsync(result);
        }
    }

    private int GetStatusCodeForException(Exception ex)
    {
        return ex switch
        {
            CoreBusinessException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}
