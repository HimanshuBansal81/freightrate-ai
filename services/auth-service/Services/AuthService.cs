using auth_service.Data;
using auth_service.Entities;
using auth_service.Models;
using Microsoft.EntityFrameworkCore;

namespace auth_service.Services;

public sealed class AuthService(
    AuthDbContext dbContext,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IAuthService
{
    private const string DefaultRole = "Shipper";
    private static readonly HashSet<string> AllowedRoles = new(StringComparer.OrdinalIgnoreCase) { "Admin", "Shipper" };

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            throw new ArgumentException("FullName is required.", nameof(request.FullName));
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Password is required.", nameof(request.Password));
        }

        var roleName = string.IsNullOrWhiteSpace(request.Role) ? DefaultRole : request.Role.Trim();
        if (!AllowedRoles.Contains(roleName))
        {
            throw new ArgumentException("Role must be Admin or Shipper.", nameof(request.Role));
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var role = await dbContext.Roles.SingleOrDefaultAsync(
            existingRole => existingRole.Name.ToLower() == roleName.ToLowerInvariant(),
            cancellationToken);

        if (role is null)
        {
            throw new InvalidOperationException("Requested role is not configured.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            UserRoles = new List<UserRole>
            {
                new() { Role = role }
            }
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(user, new[] { role.Name });
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users
            .Include(existingUser => existingUser.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(existingUser => existingUser.Email == normalizedEmail && existingUser.IsActive, cancellationToken);

        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        var roles = user.UserRoles.Select(userRole => userRole.Role.Name).ToArray();
        return BuildAuthResponse(user, roles);
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(existingUser => existingUser.UserRoles)
            .ThenInclude(userRole => userRole.Role)
            .SingleOrDefaultAsync(existingUser => existingUser.Id == userId && existingUser.IsActive, cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new CurrentUserResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Roles = user.UserRoles.Select(userRole => userRole.Role.Name).ToArray()
        };
    }

    private AuthResponse BuildAuthResponse(User user, IReadOnlyCollection<string> roles)
    {
        var token = jwtTokenService.CreateToken(user, roles);

        return new AuthResponse
        {
            Token = token.Token,
            Email = user.Email,
            FullName = user.FullName,
            Roles = roles,
            ExpiresAt = token.ExpiresAt
        };
    }
}
