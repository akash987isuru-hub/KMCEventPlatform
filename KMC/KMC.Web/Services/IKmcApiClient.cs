using KMC.Web.Models.Auth;
using KMC.Web.Models.Events;
using KMC.Web.Models.Organizer;
using KMC.Web.Models.Payments;
using KMC.Web.Models.PartnerBookings;
using KMC.Web.Models.Registrations;

namespace KMC.Web.Services;

public interface IKmcApiClient
{
    Task<IReadOnlyList<EventViewModel>> GetEventsAsync(
        EventSearchViewModel filters,
        CancellationToken cancellationToken = default);

    Task<EventViewModel?> GetEventByIdAsync(
        int eventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PartnerEventViewModel>> GetPartnerEventsAsync(
        EventSearchViewModel filters,
        CancellationToken cancellationToken = default);

    Task<PartnerEventViewModel?> GetPartnerEventByIdAsync(
        int partnerEventId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PartnerEventViewModel>> GetManagedPartnerEventsAsync(
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<PartnerEventViewModel?> GetManagedPartnerEventByIdAsync(
        int partnerEventId,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<PartnerEventApiResult> UpdatePartnerEventAsync(
        int partnerEventId,
        PartnerEventFormViewModel model,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<PartnerEventApiResult> DeletePartnerEventAsync(
        int partnerEventId,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PartnerBookingViewModel>> GetPartnerEventBookingsAsync(
        int partnerEventId,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<PartnerBookingApiResult> PurchasePartnerTicketAsync(
        PartnerTicketCheckoutViewModel model,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<PartnerBookingViewModel?> GetPartnerBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default);

    Task<PartnerBookingApiResult> CancelPartnerBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default);

    Task<AuthApiResult> RegisterAsync(
        RegisterViewModel model,
        CancellationToken cancellationToken = default);

    Task<AuthApiResult> LoginAsync(
        LoginViewModel model,
        CancellationToken cancellationToken = default);

    Task<RegistrationApiResult> PurchaseTicketAsync(
        TicketCheckoutViewModel model,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RegistrationViewModel>>
        GetMyRegistrationsAsync(
            string jwtToken,
            CancellationToken cancellationToken = default);

    Task<RegistrationApiResult> CancelRegistrationAsync(
        int registrationId,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventViewModel>> GetMyEventsAsync(
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<EventApiResult> CreateEventAsync(
        EventFormViewModel model,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<EventApiResult> UpdateEventAsync(
        int eventId,
        EventFormViewModel model,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<EventApiResult> DeleteEventAsync(
        int eventId,
        string jwtToken,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventParticipantViewModel>>
        GetEventParticipantsAsync(
            int eventId,
            string jwtToken,
            CancellationToken cancellationToken = default);
}
