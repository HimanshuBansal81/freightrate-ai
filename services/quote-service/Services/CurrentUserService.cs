using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace quote_service.Services;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int UserId => TryGetUserId(out var userId)
        ? userId
        : throw new InvalidOperationException("Authenticated user id claim is missing or invalid.");

    public string? Email => FindFirstValue(ClaimTypes.Email)
        ?? FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? FindFirstValue("email");

    public IReadOnlyCollection<string> Roles => httpContextAccessor.HttpContext?.User.Claims
        .Where(claim => claim.Type == ClaimTypes.Role || claim.Type == "role" || claim.Type == "roles")
        .Select(claim => claim.Value)
        .Where(role => !string.IsNullOrWhiteSpace(role))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray() ?? [];

    public bool IsAdmin => Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);

    private bool TryGetUserId(out int userId)
    {
        var value = FindFirstValue(ClaimTypes.NameIdentifier)
            ?? FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? FindFirstValue("sub")
            ?? FindFirstValue("userId")
            ?? FindFirstValue("id");

        return int.TryParse(value, out userId);
    }

    private string? FindFirstValue(string claimType)
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(claimType);
    }
}
