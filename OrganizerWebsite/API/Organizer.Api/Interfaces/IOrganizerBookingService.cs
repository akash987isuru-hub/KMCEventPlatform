using Organizer.Api.DTOs;

namespace Organizer.Api.Interfaces;

public interface IOrganizerBookingService
{
    Task<OrganizerBookingResponseDto> PurchaseAsync(
        string externalEventCode,
        PurchaseOrganizerTicketRequestDto request,
        CancellationToken cancellationToken = default);

    Task<OrganizerBookingResponseDto?> GetByReferenceAsync(
        string bookingReference,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizerBookingResponseDto>> GetAllAsync(
        int? eventId = null,
        CancellationToken cancellationToken = default);

    Task<OrganizerBookingResponseDto?> CancelAsync(
        string bookingReference,
        CancellationToken cancellationToken = default);
}
