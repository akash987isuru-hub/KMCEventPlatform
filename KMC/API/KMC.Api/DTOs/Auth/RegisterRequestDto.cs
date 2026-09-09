using System.ComponentModel.DataAnnotations;

namespace KMC.Api.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must contain at least 8 characters.")]
    [MaxLength(128)]
    [RegularExpression(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage =
            "Password must include uppercase, lowercase and numeric characters.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    public PublicRegistrationRole? Role { get; set; }
}