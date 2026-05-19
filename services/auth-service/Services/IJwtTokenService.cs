using auth_service.Entities;
using auth_service.Models;

namespace auth_service.Services;

public interface IJwtTokenService
{
    JwtTokenResult CreateToken(User user, IReadOnlyCollection<string> roles);
}
