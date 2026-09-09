namespace KMC.Web.Models.Organizer;

public class EventParticipantViewModel
{
    public int RegistrationId { get; set; }

    public int EventId { get; set; }

    public int ParticipantId { get; set; }

    public string ParticipantName { get; set; } = string.Empty;

    public string ParticipantEmail { get; set; } = string.Empty;

    public string TicketTierName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal TicketPrice { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentStatus { get; set; } = string.Empty;

    public string TransactionReference { get; set; } = string.Empty;

    public DateTime RegisteredAt { get; set; }

    public string Status { get; set; } = string.Empty;
}
