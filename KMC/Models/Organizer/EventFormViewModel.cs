using System.ComponentModel.DataAnnotations;

namespace KMC.Web.Models.Organizer;

public class EventFormViewModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime EventDate { get; set; } =
        DateTime.Today.AddDays(1);

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime EndDate { get; set; } =
        DateTime.Today.AddDays(1);

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "Start time")]
    public TimeSpan StartTime { get; set; } =
        new(9, 0, 0);

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "End time")]
    public TimeSpan EndTime { get; set; } =
        new(11, 0, 0);

    [Required]
    [MaxLength(150)]
    public string Venue { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(Draft|Published|Cancelled|Completed)$",
        ErrorMessage = "Select a valid event status.")]
    public string Status { get; set; } = "Published";

    [MinLength(3)]
    [MaxLength(3)]
    public List<TicketTierFormViewModel> TicketTiers { get; set; }
        = CreateDefaultTicketTiers();

    public int Capacity => TicketTiers.Sum(
        ticketTier => ticketTier.Capacity);

    public static List<TicketTierFormViewModel>
        CreateDefaultTicketTiers()
    {
        return
        [
            new()
            {
                Name = "Standard",
                Price = 1500,
                Capacity = 100,
                SortOrder = 1
            },
            new()
            {
                Name = "Premium",
                Price = 3000,
                Capacity = 50,
                SortOrder = 2
            },
            new()
            {
                Name = "VIP",
                Price = 5000,
                Capacity = 20,
                SortOrder = 3
            }
        ];
    }
}
