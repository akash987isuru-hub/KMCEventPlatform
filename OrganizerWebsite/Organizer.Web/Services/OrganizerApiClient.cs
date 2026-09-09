using System.Net;
using System.Net.Http.Json;
using Organizer.Web.Models;

namespace Organizer.Web.Services;

public class OrganizerApiClient : IOrganizerApiClient
{
    private readonly HttpClient _httpClient;

    public OrganizerApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<OrganizerEventViewModel>> GetEventsAsync(
        CancellationToken cancellationToken = default) =>
        await _httpClient.GetFromJsonAsync<List<OrganizerEventViewModel>>(
            "api/Events",
            cancellationToken) ?? [];

    public async Task<OrganizerEventViewModel?> GetEventAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"api/Events/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrganizerEventViewModel>(
            cancellationToken: cancellationToken);
    }

    public Task<OrganizerApiResult> CreateEventAsync(
        EventFormViewModel model,
        CancellationToken cancellationToken = default) =>
        SendEventAsync(HttpMethod.Post, "api/Events", model, "Event added successfully.", cancellationToken);

    public Task<OrganizerApiResult> UpdateEventAsync(
        int id,
        EventFormViewModel model,
        CancellationToken cancellationToken = default) =>
        SendEventAsync(HttpMethod.Put, $"api/Events/{id}", model, "Event updated successfully.", cancellationToken);

    public async Task<OrganizerApiResult> DeleteEventAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"api/Events/{id}", cancellationToken);
        return response.IsSuccessStatusCode
            ? OrganizerApiResult.Success(null, "Event deleted successfully.")
            : OrganizerApiResult.Failure(await ReadErrorAsync(response, "Event could not be deleted.", cancellationToken));
    }

    public async Task<OrganizerApiResult> PublishToKmcAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            $"api/Events/{id}/publish-to-kmc",
            null,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return OrganizerApiResult.Failure(
                await ReadErrorAsync(response, "The event could not be published to KMC.", cancellationToken));
        }

        var eventItem = await response.Content.ReadFromJsonAsync<OrganizerEventViewModel>(
            cancellationToken: cancellationToken);
        return OrganizerApiResult.Success(eventItem, "Event and live ticket availability synchronized with KMC.");
    }

    public async Task<OrganizerBookingApiResult> PurchaseTicketAsync(
        OrganizerCheckoutViewModel model,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            model.TicketTierId,
            model.CustomerName,
            model.CustomerEmail,
            model.CustomerPhone,
            model.Quantity,
            model.CardHolderName,
            model.CardNumber,
            model.ExpiryMonth,
            model.ExpiryYear,
            model.Cvv,
            bookingSource = "Sparkling Events Kandy"
        };

        using var response = await _httpClient.PostAsJsonAsync(
            $"api/Bookings/events/{Uri.EscapeDataString(model.ExternalEventCode)}",
            body,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return OrganizerBookingApiResult.Failure(
                await ReadErrorAsync(response, "Your ticket could not be booked.", cancellationToken));
        }

        var booking = await response.Content.ReadFromJsonAsync<OrganizerBookingViewModel>(
            cancellationToken: cancellationToken);
        return booking is null
            ? OrganizerBookingApiResult.Failure("The booking service returned an empty response.")
            : OrganizerBookingApiResult.Success(booking);
    }

    public async Task<OrganizerBookingViewModel?> GetBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/Bookings/{Uri.EscapeDataString(bookingReference)}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrganizerBookingViewModel>(
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<OrganizerBookingViewModel>> GetBookingsAsync(
        int? eventId = null,
        CancellationToken cancellationToken = default)
    {
        var endpoint = eventId.HasValue
            ? $"api/Bookings?eventId={eventId.Value}"
            : "api/Bookings";
        return await _httpClient.GetFromJsonAsync<List<OrganizerBookingViewModel>>(
            endpoint,
            cancellationToken) ?? [];
    }

    public async Task<OrganizerBookingApiResult> CancelBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(
            $"api/Bookings/{Uri.EscapeDataString(bookingReference)}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return OrganizerBookingApiResult.Failure(
                await ReadErrorAsync(response, "The booking could not be cancelled.", cancellationToken));
        }

        var booking = await response.Content.ReadFromJsonAsync<OrganizerBookingViewModel>(
            cancellationToken: cancellationToken);
        return booking is null
            ? OrganizerBookingApiResult.Failure("The cancellation response was empty.")
            : OrganizerBookingApiResult.Success(booking);
    }

    private async Task<OrganizerApiResult> SendEventAsync(
        HttpMethod method,
        string endpoint,
        EventFormViewModel model,
        string successMessage,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, endpoint)
        {
            Content = JsonContent.Create(new
            {
                title = model.Title.Trim(),
                description = model.Description?.Trim(),
                eventType = model.EventType.Trim(),
                eventDate = model.EventDate,
                endDate = model.EndDate,
                startTime = model.StartTime,
                endTime = model.EndTime,
                venue = model.Venue.Trim(),
                location = model.Location.Trim(),
                organizerName = model.OrganizerName.Trim(),
                isPublished = model.IsPublished,
                ticketTiers = model.TicketTiers.Select(item => new
                {
                    item.Id,
                    name = item.Name.Trim(),
                    description = item.Description?.Trim(),
                    item.Price,
                    item.Capacity,
                    item.SortOrder
                })
            })
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return OrganizerApiResult.Failure(
                await ReadErrorAsync(response, "The event could not be saved.", cancellationToken));
        }

        var eventItem = await response.Content.ReadFromJsonAsync<OrganizerEventViewModel>(
            cancellationToken: cancellationToken);
        if (eventItem is null)
        {
            return OrganizerApiResult.Failure("The API returned an empty response.");
        }

        var message = !eventItem.IsPublished
            ? successMessage + " It is saved as a private draft."
            : eventItem.IsPublishedToKmc
                ? successMessage + " Event details and ticket stock are live on both websites."
                : successMessage + " KMC synchronization is pending.";

        return OrganizerApiResult.Success(eventItem, message);
    }

    private static async Task<string> ReadErrorAsync(
        HttpResponseMessage response,
        string fallback,
        CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(
                cancellationToken: cancellationToken);
            return string.IsNullOrWhiteSpace(error?.Message) ? fallback : error.Message;
        }
        catch
        {
            return fallback;
        }
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
