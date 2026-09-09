using System.ComponentModel.DataAnnotations;

namespace KMC.Web.Models.Organizer;

public class PartnerEventFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Display(Name = "Event type")]
    [Required, StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Display(Name = "Start date")]
    [DataType(DataType.Date)]
    public DateTime EventDate { get; set; }

    [Display(Name = "End date")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Display(Name = "Start time")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime { get; set; }

    [Display(Name = "End time")]
    [DataType(DataType.Time)]
    public TimeSpan EndTime { get; set; }

    [Required, StringLength(150)]
    public string Venue { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Display(Name = "Publishing status")]
    public bool IsPublished { get; set; }

    public List<PartnerTicketTierFormViewModel> TicketTiers { get; set; } = [];
}

public class PartnerTicketTierFormViewModel
{
    public int ExternalTicketTierId { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    [StringLength(180)]
    public string? Description { get; set; }

    [Range(0, 1000000)]
    public decimal Price { get; set; }

    [Range(1, 100000)]
    public int Capacity { get; set; }

    public int SoldCount { get; set; }

    [Range(1, 5)]
    public int SortOrder { get; set; }
}
