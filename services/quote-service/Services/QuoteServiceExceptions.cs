namespace quote_service.Services;

public sealed class QuoteValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Quote request validation failed.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

public sealed class QuoteBusinessException(string message) : Exception(message);
