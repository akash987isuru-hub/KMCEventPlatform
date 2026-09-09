namespace KMC.OrganizerDesktop.Models
{
    public class EventResponse
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

        public List<TicketTierResponse> TicketTiers { get; set; }
            = new List<TicketTierResponse>();
    }
}