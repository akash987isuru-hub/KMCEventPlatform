namespace KMC.Web.Models.Events;

public class EventViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string EventType { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public DateTime EndDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string Venue { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string Status { get; set; } = string.Empty;

    public int OrganizerId { get; set; }

    public string OrganizerName { get; set; } = string.Empty;

    public int RegisteredParticipantCount { get; set; }

    public IReadOnlyList<TicketTierViewModel> TicketTiers { get; set; }
        = Array.Empty<TicketTierViewModel>();

    public decimal MinimumTicketPrice => TicketTiers.Count == 0
        ? 0
        : TicketTiers.Min(ticketTier => ticketTier.Price);

    public decimal MaximumTicketPrice => TicketTiers.Count == 0
        ? 0
        : TicketTiers.Max(ticketTier => ticketTier.Price);

    public bool HasEnded =>
        EndDate.Date.Add(EndTime) <= DateTime.Now;

    public bool IsSoldOut =>
        TicketTiers.Count > 0 &&
        TicketTiers.Sum(ticketTier => ticketTier.RemainingCount) <= 0;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
