namespace KMC.Api.DTOs.Registrations;

public class ParticipantListResult
{
    public RegistrationOperationStatus Status { get; set; }

    public string? Message { get; set; }

    public IReadOnlyList<ParticipantResponseDto> Participants { get; set; }
        = Array.Empty<ParticipantResponseDto>();
}