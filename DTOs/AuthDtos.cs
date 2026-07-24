using System.ComponentModel.DataAnnotations;

namespace SafeVault.Api.DTOs;

public class RegisterDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 12)]
    public required string Password { get; set; }

    // Allows designating if the user being registered should be an Admin or regular User
    [Required]
    [RegularExpression("^(User|Admin)$", ErrorMessage = "Role must be either 'User' or 'Admin'.")]
    public required string Role { get; set; }
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}

public class AuthResponseDto
{
    public required string Token { get; set; }
    public DateTime Expiration { get; set; }
    public required string Role { get; set; }
}   