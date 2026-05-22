namespace quote_service.Models;

public sealed class ErrorDetail
{
    public required string Field { get; set; }
    public required string Issue { get; set; }
}
