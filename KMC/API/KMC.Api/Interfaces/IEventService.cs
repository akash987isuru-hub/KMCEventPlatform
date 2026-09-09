using KMC.Api.DTOs.Events;

namespace KMC.Api.Interfaces;

public interface IEventService
{
    Task<EventOperationResult> CreateAsync(
        CreateEventRequestDto request,
        int organizerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventResponseDto>> GetAllAsync(
        EventSearchRequestDto search,
        CancellationToken cancellationToken = default);

    Task<EventResponseDto?> GetByIdAsync(
        int eventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventResponseDto>> GetOrganizerEventsAsync(
        int organizerId,
        CancellationToken cancellationToken = default);

    Task<EventOperationResult> UpdateAsync(
        int eventId,
        UpdateEventRequestDto request,
        int organizerId,
        CancellationToken cancellationToken = default);

    Task<EventOperationResult> DeleteAsync(
        int eventId,
        int organizerId,
        CancellationToken cancellationToken = default);
}