namespace quote_service.Services;

public static class QuoteErrorCodes
{
    public const string InvalidPincode = "INVALID_PINCODE";
    public const string UnserviceableRoute = "UNSERVICEABLE_ROUTE";
    public const string NoActiveRateRules = "NO_ACTIVE_RATE_RULES";
    public const string InvalidWeight = "INVALID_WEIGHT";
    public const string InvalidDimensions = "INVALID_DIMENSIONS";
    public const string InvalidPreference = "INVALID_PREFERENCE";
    public const string QuoteNotFound = "QUOTE_NOT_FOUND";
    public const string AiServiceUnavailable = "AI_SERVICE_UNAVAILABLE";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string BadRequest = "BAD_REQUEST";
}
