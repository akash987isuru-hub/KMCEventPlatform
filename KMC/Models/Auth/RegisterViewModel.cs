using System.ComponentModel.DataAnnotations;

namespace KMC.Web.Models.Auth;

public class RegisterViewModel
{
    [Required]
    [MaxLength(100)]
    [Display(Name = "Full name")]
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
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(
        nameof(Password),
        ErrorMessage = "The passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;
}