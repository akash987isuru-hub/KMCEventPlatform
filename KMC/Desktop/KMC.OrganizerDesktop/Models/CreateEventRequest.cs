namespace KMC.OrganizerDesktop.Models
{
    public class CreateEventRequest
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string EventType { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public string Venue { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string Status { get; set; } = "Published";

        public List<TicketTierRequest> TicketTiers { get; set; }
            = new List<TicketTierRequest>();
    }
}