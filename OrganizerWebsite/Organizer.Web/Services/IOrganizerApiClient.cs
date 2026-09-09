using Organizer.Web.Models;

namespace Organizer.Web.Services;

public interface IOrganizerApiClient
{
    Task<IReadOnlyList<OrganizerEventViewModel>> GetEventsAsync(
        CancellationToken cancellationToken = default);
    Task<OrganizerEventViewModel?> GetEventAsync(
        int id,
        CancellationToken cancellationToken = default);
    Task<OrganizerApiResult> CreateEventAsync(
        EventFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<OrganizerApiResult> UpdateEventAsync(
        int id,
        EventFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<OrganizerApiResult> DeleteEventAsync(
        int id,
        CancellationToken cancellationToken = default);
    Task<OrganizerApiResult> PublishToKmcAsync(
        int id,
        CancellationToken cancellationToken = default);
    Task<OrganizerBookingApiResult> PurchaseTicketAsync(
        OrganizerCheckoutViewModel model,
        CancellationToken cancellationToken = default);
    Task<OrganizerBookingViewModel?> GetBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrganizerBookingViewModel>> GetBookingsAsync(
        int? eventId = null,
        CancellationToken cancellationToken = default);
    Task<OrganizerBookingApiResult> CancelBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default);
}
