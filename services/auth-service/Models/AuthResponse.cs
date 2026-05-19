namespace auth_service.Models;

public sealed class AuthResponse
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required IReadOnlyCollection<string> Roles { get; set; }
    public DateTime ExpiresAt { get; set; }
}
