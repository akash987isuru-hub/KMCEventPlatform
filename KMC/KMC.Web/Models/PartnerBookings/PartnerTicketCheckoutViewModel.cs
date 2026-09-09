using System.ComponentModel.DataAnnotations;

namespace KMC.Web.Models.PartnerBookings;

public class PartnerTicketCheckoutViewModel
{
    public int PartnerEventId { get; set; }

    [Range(1, int.MaxValue)]
    public int TicketTierId { get; set; }

    [Range(1, 10)]
    public int Quantity { get; set; } = 1;

    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Name on card")]
    public string CardHolderName { get; set; } = string.Empty;

    [Required, StringLength(19)]
    [RegularExpression(@"^\d{4}(?:[ -]?\d{4}){3}$", ErrorMessage = "Enter exactly 16 digits in four groups of four.")]
    [Display(Name = "Card number")]
    public string CardNumber { get; set; } = string.Empty;

    [Range(1, 12)]
    [Display(Name = "Expiry month")]
    public int ExpiryMonth { get; set; } = DateTime.Today.Month;

    [Range(2026, 2100)]
    [Display(Name = "Expiry year")]
    public int ExpiryYear { get; set; } = DateTime.Today.Year + 1;

    [Required, RegularExpression("^[0-9]{3}$")]
    public string Cvv { get; set; } = string.Empty;
}
