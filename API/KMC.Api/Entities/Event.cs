using System.ComponentModel.DataAnnotations;

namespace KMC.Api.Entities;

public class Event
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public DateTime EndDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    [Required]
    [MaxLength(150)]
    public string Venue { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Draft;

    public int OrganizerId { get; set; }

    public User Organizer { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<TicketTier> TicketTiers { get; set; }
        = new List<TicketTier>();

    public ICollection<Registration> Registrations { get; set; }
        = new List<Registration>();
}
