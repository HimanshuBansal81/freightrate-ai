using System.ComponentModel.DataAnnotations;

namespace quote_service.Models;

public sealed class QuoteCompareRequest
{
    [Required]
    public string? OriginPincode { get; set; }

    [Required]
    public string? DestinationPincode { get; set; }

    public decimal ActualWeightKg { get; set; }
    public decimal LengthCm { get; set; }
    public decimal WidthCm { get; set; }
    public decimal HeightCm { get; set; }
    public string? Preference { get; set; }
}
