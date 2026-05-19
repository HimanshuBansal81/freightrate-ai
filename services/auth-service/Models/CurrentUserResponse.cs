namespace auth_service.Models;

public sealed class CurrentUserResponse
{
    public int UserId { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public required IReadOnlyCollection<string> Roles { get; set; }
}
