using System.ComponentModel.DataAnnotations;

namespace KMC.Api.DTOs.PartnerEvents;

public class PartnerTicketPurchaseRequestDto
{
    [Range(1, int.MaxValue)]
    public int TicketTierId { get; set; }

    [StringLength(100)]
    public string? CustomerName { get; set; }

    [EmailAddress, StringLength(150)]
    public string? CustomerEmail { get; set; }

    [StringLength(30)]
    public string? CustomerPhone { get; set; }

    [Range(1, 10)]
    public int Quantity { get; set; } = 1;

    [Required, StringLength(100, MinimumLength = 2)]
    public string CardHolderName { get; set; } = string.Empty;

    [Required, StringLength(19)]
    [RegularExpression(@"^\d{4}(?:[ -]?\d{4}){3}$", ErrorMessage = "Enter exactly 16 digits in four groups of four.")]
    public string CardNumber { get; set; } = string.Empty;

    [Range(1, 12)]
    public int ExpiryMonth { get; set; }

    [Range(2026, 2100)]
    public int ExpiryYear { get; set; }

    [Required, RegularExpression("^[0-9]{3}$")]
    public string Cvv { get; set; } = string.Empty;
}
