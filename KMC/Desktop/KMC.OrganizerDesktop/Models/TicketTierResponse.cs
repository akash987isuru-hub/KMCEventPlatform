namespace KMC.OrganizerDesktop.Models
{
    public class TicketTierResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Capacity { get; set; }

        public int SortOrder { get; set; }
    }
}