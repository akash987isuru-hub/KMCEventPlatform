namespace Organizer.Api.DTOs.Kmc;

public class KmcPartnerEventResponseDto
{
    public int Id { get; set; }

    public string ExternalEventCode { get; set; } = string.Empty;

    public DateTime LastSyncedAt { get; set; }
}
