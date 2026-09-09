namespace KMC.Web.Models.Events;

public class PartnerEventViewModel
{
    public int Id { get; set; }
    public string ExternalEventCode { get; set; } = string.Empty;
    public int SourceEventId { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime EndDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public string OrganizerEventUrl { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public int TotalCapacity { get; set; }
    public int RegisteredParticipantCount { get; set; }
    public int AvailableCount { get; set; }
    public decimal MinimumTicketPrice { get; set; }
    public IReadOnlyList<PartnerEventTicketTierViewModel> TicketTiers { get; set; }
        = Array.Empty<PartnerEventTicketTierViewModel>();
    public bool HasEnded =>
        EndDate.Date.Add(EndTime) <= DateTime.Now;
    public bool IsSoldOut =>
        TicketTiers.Count > 0 &&
        TicketTiers.Sum(ticketTier => ticketTier.AvailableCount) <= 0;
    public DateTime LastSyncedAt { get; set; }
}
