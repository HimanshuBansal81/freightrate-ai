namespace auth_service.Models;

public sealed class JwtTokenResult
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}
