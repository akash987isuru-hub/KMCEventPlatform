using System.ComponentModel.DataAnnotations;

namespace KMC.Api.DTOs.PartnerEvents;

public class PartnerEventSyncRequestDto
{
    [Required, StringLength(100)]
    public string ExternalEventCode { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int SourceEventId { get; set; }

    [Required, StringLength(100)]
    public string SourceSystem { get; set; } = "Organizer.Api";

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required, StringLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required, StringLength(150)]
    public string Venue { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string OrganizerName { get; set; } = string.Empty;

    [Required, Url, StringLength(500)]
    public string OrganizerEventUrl { get; set; } = string.Empty;

    public bool IsPublished { get; set; } = true;

    [Required, MinLength(1), MaxLength(5)]
    public List<PartnerEventTicketTierSyncDto> TicketTiers { get; set; } = [];
}
