using System.ComponentModel.DataAnnotations;

namespace auth_service.Models;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
