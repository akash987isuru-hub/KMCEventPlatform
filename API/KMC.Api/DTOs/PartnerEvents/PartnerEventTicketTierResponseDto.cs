namespace KMC.Api.DTOs.PartnerEvents;

public class PartnerEventTicketTierResponseDto
{
    public int Id { get; set; }
    public int ExternalTicketTierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Capacity { get; set; }
    public int SoldCount { get; set; }
    public int AvailableCount => Math.Max(0, Capacity - SoldCount);
    public int SortOrder { get; set; }
}
