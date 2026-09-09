using System.ComponentModel.DataAnnotations;

namespace Organizer.Api.DTOs;

public class PurchaseOrganizerTicketRequestDto
{
    [Range(1, int.MaxValue)]
    public int TicketTierId { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required, StringLength(30, MinimumLength = 7)]
    public string CustomerPhone { get; set; } = string.Empty;

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

    [StringLength(30)]
    public string BookingSource { get; set; } = "Sparkling Events Kandy";
}
