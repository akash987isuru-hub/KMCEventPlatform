using System.ComponentModel.DataAnnotations;

namespace KMC.Api.DTOs.PartnerEvents;

public class PartnerEventUpdateRequestDto
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

    public bool IsPublished { get; set; }

    [Required, MinLength(1), MaxLength(5)]
    public List<PartnerEventTicketTierUpdateDto> TicketTiers { get; set; } = [];
}

public class PartnerEventTicketTierUpdateDto
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

    [Range(1, 5)]
    public int SortOrder { get; set; }
}
