using System.ComponentModel.DataAnnotations;

namespace Organizer.Api.Entities;

public class OrganizerEvent
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string ExternalEventCode { get; set; } = Guid.NewGuid().ToString();

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

    public bool IsPublished { get; set; } = true;
    public bool IsPublishedToKmc { get; set; }
    public int? KmcEventId { get; set; }

    [MaxLength(500)]
    public string? KmcSyncError { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedToKmcAt { get; set; }

    public ICollection<OrganizerTicketTier> TicketTiers { get; set; }
        = new List<OrganizerTicketTier>();

    public ICollection<OrganizerBooking> Bookings { get; set; }
        = new List<OrganizerBooking>();
}
