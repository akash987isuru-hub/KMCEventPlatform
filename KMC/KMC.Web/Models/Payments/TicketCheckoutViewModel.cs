using System.ComponentModel.DataAnnotations;
using KMC.Web.Models.Events;

namespace KMC.Web.Models.Payments;

public class TicketCheckoutViewModel : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int EventId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Select a ticket category.")]
    [Display(Name = "Ticket category")]
    public int TicketTierId { get; set; }

    [Range(1, 10, ErrorMessage = "Choose between 1 and 10 tickets.")]
    [Display(Name = "Number of tickets")]
    public int Quantity { get; set; } = 1;

    [Required]
    [MinLength(2, ErrorMessage = "Enter the card holder name.")]
    [MaxLength(100)]
    [Display(Name = "Card holder name")]
    public string CardHolderName { get; set; } = string.Empty;

    [Required]
    [MaxLength(19)]
    [RegularExpression(
        @"^(?:\d[ -]?){15}\d$",
        ErrorMessage = "Card number must contain exactly 16 digits.")]
    [Display(Name = "Card number")]
    public string CardNumber { get; set; } = string.Empty;

    [Range(1, 12)]
    [Display(Name = "Expiry month")]
    public int ExpiryMonth { get; set; } = DateTime.Today.Month;

    [Range(2026, 2100)]
    [Display(Name = "Expiry year")]
    public int ExpiryYear { get; set; } = DateTime.Today.Year + 1;

    [Required]
    [RegularExpression(
        "^[0-9]{3}$",
        ErrorMessage = "CVV must contain exactly 3 digits.")]
    [Display(Name = "CVV")]
    public string Cvv { get; set; } = string.Empty;

    public EventViewModel? Event { get; set; }

    public TicketTierViewModel? SelectedTicket { get; set; }

    public decimal TotalAmount =>
        SelectedTicket is null
            ? 0
            : SelectedTicket.Price * Quantity;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (SelectedTicket is not null &&
            Quantity > SelectedTicket.RemainingCount)
        {
            yield return new ValidationResult(
                $"Only {SelectedTicket.RemainingCount} ticket(s) are available.",
                [nameof(Quantity)]);
        }

        if (!string.IsNullOrWhiteSpace(CardNumber))
        {
            if (CardNumber.Any(character =>
                    !char.IsDigit(character) &&
                    character != ' ' && character != '-'))
            {
                yield return new ValidationResult(
                    "Card number may contain only digits, spaces or hyphens.",
                    [nameof(CardNumber)]);
            }
            else
            {
                var normalizedCard = new string(
                    CardNumber.Where(char.IsDigit).ToArray());

                if (normalizedCard.Length != 16)
                {
                    yield return new ValidationResult(
                        "Card number must contain exactly 16 digits.",
                        [nameof(CardNumber)]);
                }
            }
        }

        if (ExpiryMonth is < 1 or > 12 ||
            ExpiryYear is < 1 or > 9999)
        {
            yield return new ValidationResult(
                "Enter a valid expiry month and year.",
                [nameof(ExpiryMonth), nameof(ExpiryYear)]);
            yield break;
        }

        var expiry = new DateTime(ExpiryYear, ExpiryMonth, 1);
        var currentMonth = new DateTime(
            DateTime.Today.Year,
            DateTime.Today.Month,
            1);

        if (expiry < currentMonth)
        {
            yield return new ValidationResult(
                "The card expiry date has passed.",
                [nameof(ExpiryMonth), nameof(ExpiryYear)]);
        }
    }
}
