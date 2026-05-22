using quote_service.Models;
using quote_service.Services;

namespace quote_service.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (QuoteApiException exception)
        {
            logger.LogWarning(exception, "Handled quote API exception {ErrorCode}.", exception.ErrorCode);
            await WriteErrorAsync(
                context,
                exception.StatusCode,
                exception.ErrorCode,
                exception.Message,
                exception.Details);
        }
        catch (BadHttpRequestException exception)
        {
            logger.LogWarning(exception, "Malformed request.");
            await WriteErrorAsync(
                context,
                StatusCodes.Status400BadRequest,
                QuoteErrorCodes.BadRequest,
                "The request is malformed.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception.");
            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                QuoteErrorCodes.InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string errorCode,
        string message,
        IReadOnlyCollection<ErrorDetail>? details = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = StandardErrorResponseFactory.Create(context, statusCode, errorCode, message, details);
        await context.Response.WriteAsJsonAsync(response);
    }
}
