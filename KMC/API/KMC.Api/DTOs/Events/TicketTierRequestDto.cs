using System.ComponentModel.DataAnnotations;

namespace KMC.Api.DTOs.Events;

public class TicketTierRequestDto
{
    public int Id { get; set; }

    [Required]
    [MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1000000)]
    public decimal Price { get; set; }

    [Range(1, 100000)]
    public int Capacity { get; set; }

    [Range(1, 3)]
    public int SortOrder { get; set; }
}
