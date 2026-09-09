using System.ComponentModel.DataAnnotations;
using KMC.Api.Entities;

namespace KMC.Api.DTOs.Events;

public class UpdateEventRequestDto
{
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    [MaxLength(150)]
    public string Venue { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    public EventStatus Status { get; set; }

    [Required]
    [MinLength(3)]
    [MaxLength(3)]
    public List<TicketTierRequestDto> TicketTiers { get; set; } = [];
}
