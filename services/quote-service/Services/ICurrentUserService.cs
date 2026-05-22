namespace quote_service.Services;

public interface ICurrentUserService
{
    int UserId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsAdmin { get; }
}
