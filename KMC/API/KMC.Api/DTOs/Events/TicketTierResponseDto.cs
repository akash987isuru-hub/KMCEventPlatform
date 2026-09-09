namespace KMC.Api.DTOs.Events;

public class TicketTierResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Capacity { get; set; }

    public int SoldCount { get; set; }

    public int RemainingCount => Math.Max(0, Capacity - SoldCount);

    public int SortOrder { get; set; }
}
