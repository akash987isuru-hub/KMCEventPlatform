using System.ComponentModel.DataAnnotations;

namespace KMC.Api.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}