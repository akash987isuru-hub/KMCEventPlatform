using KMC.Api.DTOs.PartnerEvents;

namespace KMC.Api.Interfaces;

public interface IPartnerBookingService
{
    Task<PartnerBookingOperationResult> PurchaseAsync(
        int partnerEventId,
        PartnerTicketPurchaseRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PartnerBookingListResult> GetBookingsAsync(
        int partnerEventId,
        string organizerName,
        CancellationToken cancellationToken = default);

    Task<PartnerBookingResponseDto?> GetBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default);

    Task<PartnerBookingOperationResult> CancelBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default);
}
