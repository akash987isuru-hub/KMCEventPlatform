using KMC.Api.DTOs.PartnerEvents;
using KMC.Api.Entities;

namespace KMC.Api.Interfaces;

public interface IOrganizerPartnerBookingClient
{
    Task<PartnerBookingOperationResult> PurchaseAsync(
        PartnerEvent eventItem,
        PartnerEventTicketTier ticketTier,
        PartnerTicketPurchaseRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PartnerBookingResponseDto>> GetBookingsAsync(
        int sourceEventId,
        CancellationToken cancellationToken = default);

    Task<PartnerBookingResponseDto?> GetBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default);

    Task<PartnerBookingOperationResult> CancelBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default);
}
