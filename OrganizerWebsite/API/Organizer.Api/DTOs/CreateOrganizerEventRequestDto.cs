using System.ComponentModel.DataAnnotations;

namespace Organizer.Api.DTOs;

public class CreateOrganizerEventRequestDto
{
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

    public bool IsPublished { get; set; } = true;

    [Required, MinLength(1), MaxLength(5)]
    public List<OrganizerTicketTierRequestDto> TicketTiers { get; set; } = [];
}
