namespace KMC.Web.Models.Events;

public class TicketTierViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Capacity { get; set; }

    public int SoldCount { get; set; }

    public int RemainingCount { get; set; }

    public int SortOrder { get; set; }
}
