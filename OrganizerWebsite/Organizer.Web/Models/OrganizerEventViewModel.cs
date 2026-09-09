namespace Organizer.Web.Models;

public class OrganizerEventViewModel
{
    public int Id { get; set; }
    public string ExternalEventCode { get; set; } = string.Empty;
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
    public bool IsPublished { get; set; }
    public bool IsPublishedToKmc { get; set; }
    public int? KmcEventId { get; set; }
    public string? KmcSyncError { get; set; }
    public int TotalCapacity { get; set; }
    public int SoldCount { get; set; }
    public int AvailableCount { get; set; }
    public decimal MinimumTicketPrice { get; set; }
    public IReadOnlyList<OrganizerTicketTierViewModel> TicketTiers { get; set; }
        = Array.Empty<OrganizerTicketTierViewModel>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedToKmcAt { get; set; }
    public bool HasEnded => EndDate.Date.Add(EndTime) <= DateTime.Now;
    public bool IsSoldOut => TicketTiers.Count > 0 && AvailableCount <= 0;
}
