using System.ComponentModel.DataAnnotations;

namespace KMC.Api.DTOs.Events;

public class EventSearchRequestDto
{
    [MaxLength(150)]
    public string? Search { get; set; }

    [MaxLength(100)]
    public string? EventType { get; set; }

    public DateTime? Date { get; set; }

    [MaxLength(150)]
    public string? Venue { get; set; }

    [MaxLength(150)]
    public string? Location { get; set; }
}
