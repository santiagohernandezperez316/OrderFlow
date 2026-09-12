using Microsoft.AspNetCore.Http;
using OrderFlow.Domain.Exceptions;
using System.Net;
using System.Text.Json;

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
                ErrorMessage = GetErrorMessage(ex)
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = GetStatusCodeForException(ex);
            await context.Response.WriteAsync(result);
        }
    }

    private static int GetStatusCodeForException(Exception ex)
    {
        return ex switch
        {
            CoreBusinessException => (int)HttpStatusCode.BadRequest,
            BadHttpRequestException badHttpRequest => badHttpRequest.StatusCode,
            JsonException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private static string GetErrorMessage(Exception ex)
    {
        // BadHttpRequestException envuelve la JsonException real con la posición del error;
        // se combina para dar un mensaje accionable en vez de solo "Failed to read parameter...".
        if (ex is BadHttpRequestException && ex.InnerException is JsonException jsonException)
        {
            return $"{ex.Message} {jsonException.Message}";
        }

        return ex.Message;
    }
}
