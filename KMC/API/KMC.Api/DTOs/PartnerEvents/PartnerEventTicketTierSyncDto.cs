using System.ComponentModel.DataAnnotations;

namespace KMC.Api.DTOs.PartnerEvents;

public class PartnerEventTicketTierSyncDto
{
    [Range(1, int.MaxValue)]
    public int ExternalTicketTierId { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    [StringLength(180)]
    public string? Description { get; set; }

    [Range(0, 1000000)]
    public decimal Price { get; set; }

    [Range(1, 100000)]
    public int Capacity { get; set; }

    [Range(0, 100000)]
    public int SoldCount { get; set; }

    [Range(1, 5)]
    public int SortOrder { get; set; }
}
