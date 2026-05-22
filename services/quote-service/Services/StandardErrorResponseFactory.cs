using System.Diagnostics;
using quote_service.Models;

namespace quote_service.Services;

public static class StandardErrorResponseFactory
{
    public static StandardErrorResponse Create(
        HttpContext httpContext,
        int statusCode,
        string errorCode,
        string message,
        IReadOnlyCollection<ErrorDetail>? details = null)
    {
        return new StandardErrorResponse
        {
            TraceId = Activity.Current?.Id ?? httpContext.TraceIdentifier,
            StatusCode = statusCode,
            ErrorCode = errorCode,
            Message = message,
            Details = details ?? [],
            Timestamp = DateTime.UtcNow
        };
    }
}
