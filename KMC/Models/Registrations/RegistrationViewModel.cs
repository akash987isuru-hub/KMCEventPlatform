namespace KMC.Web.Models.Registrations;

public class RegistrationViewModel
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public string EventTitle { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public DateTime EndDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string EventStatus { get; set; } = string.Empty;

    public int ParticipantId { get; set; }

    public string ParticipantName { get; set; } = string.Empty;

    public string ParticipantEmail { get; set; } = string.Empty;

    public int TicketTierId { get; set; }

    public string TicketTierName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal TicketPrice { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentStatus { get; set; } = string.Empty;

    public string TransactionReference { get; set; } = string.Empty;

    public string CardDisplay { get; set; } = string.Empty;

    public DateTime? PaidAt { get; set; }

    public DateTime RegisteredAt { get; set; }

    public string Status { get; set; } = string.Empty;
}
