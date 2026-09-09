namespace KMC.Api.Entities;

public class Registration
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public Event Event { get; set; } = null!;

    public int ParticipantId { get; set; }

    public User Participant { get; set; } = null!;

    public int TicketTierId { get; set; }

    public TicketTier TicketTier { get; set; } = null!;

    public int Quantity { get; set; } = 1;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public RegistrationStatus Status { get; set; }
        = RegistrationStatus.Confirmed;

    public Payment? Payment { get; set; }
}
