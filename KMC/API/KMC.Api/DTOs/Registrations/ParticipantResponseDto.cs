namespace KMC.Api.DTOs.Registrations;

public class ParticipantResponseDto
{
    public int RegistrationId { get; set; }

    public int ParticipantId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string TicketTierName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal TicketPrice { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentStatus { get; set; } = string.Empty;

    public string TransactionReference { get; set; } = string.Empty;

    public DateTime RegisteredAt { get; set; }

    public string Status { get; set; } = string.Empty;
}
