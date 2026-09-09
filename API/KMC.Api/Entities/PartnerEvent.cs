using System.ComponentModel.DataAnnotations;

namespace KMC.Api.Entities;

public class PartnerEvent
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string ExternalEventCode { get; set; } = string.Empty;

    public int SourceEventId { get; set; }

    [Required, MaxLength(100)]
    public string SourceSystem { get; set; } = "Organizer.Api";

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public DateTime EndDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    [Required, MaxLength(150)]
    public string Venue { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string OrganizerName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string OrganizerEventUrl { get; set; } = string.Empty;

    public bool IsPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PartnerEventTicketTier> TicketTiers { get; set; }
        = new List<PartnerEventTicketTier>();
}
