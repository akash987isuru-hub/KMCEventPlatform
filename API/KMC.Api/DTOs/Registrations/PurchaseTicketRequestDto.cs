using System.ComponentModel.DataAnnotations;

namespace KMC.Api.DTOs.Registrations;

public class PurchaseTicketRequestDto
{
    [Range(1, int.MaxValue)]
    public int TicketTierId { get; set; }

    [Range(1, 10, ErrorMessage = "Choose between 1 and 10 tickets.")]
    public int Quantity { get; set; } = 1;

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string CardHolderName { get; set; } = string.Empty;

    [Required]
    [MaxLength(19)]
    [RegularExpression(@"^(?:\d[ -]?){15}\d$")]
    public string CardNumber { get; set; } = string.Empty;

    [Range(1, 12)]
    public int ExpiryMonth { get; set; }

    [Range(2026, 2100)]
    public int ExpiryYear { get; set; }

    [Required]
    [RegularExpression("^[0-9]{3}$")]
    public string Cvv { get; set; } = string.Empty;
}
