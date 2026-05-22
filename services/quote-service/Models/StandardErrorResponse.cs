namespace quote_service.Models;

public sealed class StandardErrorResponse
{
    public required string TraceId { get; set; }
    public int StatusCode { get; set; }
    public required string ErrorCode { get; set; }
    public required string Message { get; set; }
    public IReadOnlyCollection<ErrorDetail> Details { get; set; } = [];
    public DateTime Timestamp { get; set; }
}
