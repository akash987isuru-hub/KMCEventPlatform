using System.ComponentModel.DataAnnotations;

namespace Organizer.Web.Models;

public class EventFormViewModel
{
    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required, StringLength(100)]
    [Display(Name = "Event type")]
    public string EventType { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(1);

    [Required, DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1);

    [Required, DataType(DataType.Time)]
    [Display(Name = "Start time")]
    public TimeSpan StartTime { get; set; } = new(9, 0, 0);

    [Required, DataType(DataType.Time)]
    [Display(Name = "End time")]
    public TimeSpan EndTime { get; set; } = new(11, 0, 0);

    [Required, StringLength(150)]
    public string Venue { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required, StringLength(150)]
    [Display(Name = "Organizer name")]
    public string OrganizerName { get; set; } = "Sparkling Events Kandy";

    [Display(Name = "Publish on both websites")]
    public bool IsPublished { get; set; } = true;

    [Required, MinLength(1), MaxLength(5)]
    public List<TicketTierFormViewModel> TicketTiers { get; set; } = CreateDefaultTiers();

    public static List<TicketTierFormViewModel> CreateDefaultTiers() =>
    [
        new()
        {
            Name = "General",
            Description = "Standard event admission",
            Price = 1500,
            Capacity = 100,
            SortOrder = 1
        },
        new()
        {
            Name = "Premium",
            Description = "Priority seating and welcome refreshment",
            Price = 2500,
            Capacity = 40,
            SortOrder = 2
        },
        new()
        {
            Name = "Patron",
            Description = "Front-row seating and a keepsake",
            Price = 4000,
            Capacity = 15,
            SortOrder = 3
        }
    ];
}
