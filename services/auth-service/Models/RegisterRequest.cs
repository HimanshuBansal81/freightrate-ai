using System.ComponentModel.DataAnnotations;

namespace auth_service.Models;

public sealed class RegisterRequest
{
    [Required]
    public required string FullName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }

    public string? Role { get; set; }
}
