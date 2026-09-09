using Organizer.Api.DTOs;

namespace Organizer.Api.Interfaces;

public interface IOrganizerEventService
{
    Task<IReadOnlyList<OrganizerEventResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<OrganizerEventResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<OrganizerEventResponseDto> CreateAsync(
        CreateOrganizerEventRequestDto request,
        CancellationToken cancellationToken = default);

    Task<OrganizerEventResponseDto?> UpdateAsync(
        int id,
        UpdateOrganizerEventRequestDto request,
        CancellationToken cancellationToken = default);

    Task<OrganizerEventResponseDto?> PublishToKmcAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}
