using KMC.Api.Infrastructure;

namespace KMC.Api.DTOs.PartnerEvents;

public class PartnerEventResponseDto
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
    public int AvailableCount => Math.Max(0, TotalCapacity - RegisteredParticipantCount);
    public decimal MinimumTicketPrice { get; set; }
    public IReadOnlyList<PartnerEventTicketTierResponseDto> TicketTiers { get; set; }
        = Array.Empty<PartnerEventTicketTierResponseDto>();
    public bool HasEnded =>
        EndDate.Date.Add(EndTime) <= KmcTime.Now;
    public bool IsSoldOut =>
        TicketTiers.Count > 0 && TicketTiers.Sum(item => item.AvailableCount) <= 0;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime LastSyncedAt { get; set; }
}
