using KMC.Api.DTOs.Registrations;

namespace KMC.Api.Interfaces;

public interface IRegistrationService
{
    Task<RegistrationOperationResult> RegisterForEventAsync(
        int eventId,
        int participantId,
        PurchaseTicketRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegistrationResponseDto>> GetMyRegistrationsAsync(
        int participantId,
        CancellationToken cancellationToken = default);

    Task<ParticipantListResult> GetEventParticipantsAsync(
        int eventId,
        int organizerId,
        CancellationToken cancellationToken = default);

    Task<RegistrationOperationResult> CancelRegistrationAsync(
        int registrationId,
        int participantId,
        CancellationToken cancellationToken = default);
}
