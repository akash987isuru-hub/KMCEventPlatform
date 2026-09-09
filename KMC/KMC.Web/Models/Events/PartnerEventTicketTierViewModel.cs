namespace KMC.Web.Models.Events;

public class PartnerEventTicketTierViewModel
{
    public int Id { get; set; }
    public int ExternalTicketTierId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Capacity { get; set; }
    public int SoldCount { get; set; }
    public int AvailableCount { get; set; }
    public int SortOrder { get; set; }
}
