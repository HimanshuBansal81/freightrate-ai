using quote_service.Models;

namespace quote_service.Services;

public class QuoteApiException(
    int statusCode,
    string errorCode,
    string message,
    IReadOnlyCollection<ErrorDetail>? details = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string ErrorCode { get; } = errorCode;
    public IReadOnlyCollection<ErrorDetail> Details { get; } = details ?? [];
}

public sealed class QuoteValidationException(
    string errorCode,
    string message,
    IReadOnlyCollection<ErrorDetail> details,
    int statusCode = StatusCodes.Status422UnprocessableEntity)
    : QuoteApiException(statusCode, errorCode, message, details);

public sealed class QuoteBusinessException(
    string errorCode,
    string message,
    IReadOnlyCollection<ErrorDetail>? details = null,
    int statusCode = StatusCodes.Status422UnprocessableEntity)
    : QuoteApiException(statusCode, errorCode, message, details);
