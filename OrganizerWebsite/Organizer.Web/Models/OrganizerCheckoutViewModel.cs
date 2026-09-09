using System.ComponentModel.DataAnnotations;

namespace Organizer.Web.Models;

public class OrganizerCheckoutViewModel
{
    public int EventId { get; set; }
    public string ExternalEventCode { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    [Display(Name = "Ticket category")]
    public int TicketTierId { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Your name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    [Display(Name = "Email address")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required, StringLength(30, MinimumLength = 7)]
    [Display(Name = "Mobile number")]
    public string CustomerPhone { get; set; } = string.Empty;

    [Range(1, 10)]
    public int Quantity { get; set; } = 1;

    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Name on card")]
    public string CardHolderName { get; set; } = string.Empty;

    [Required, StringLength(19)]
    [RegularExpression(@"^(?:\d[ -]?){15}\d$", ErrorMessage = "Enter a valid 16-digit card number.")]
    [Display(Name = "Card number")]
    public string CardNumber { get; set; } = string.Empty;

    [Range(1, 12)]
    [Display(Name = "Expiry month")]
    public int ExpiryMonth { get; set; } = DateTime.Today.Month;

    [Range(2026, 2100)]
    [Display(Name = "Expiry year")]
    public int ExpiryYear { get; set; } = DateTime.Today.Year + 1;

    [Required, RegularExpression("^[0-9]{3}$")]
    [Display(Name = "CVV")]
    public string Cvv { get; set; } = string.Empty;
}
