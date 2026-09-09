using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KMC.Web.Models.Auth;
using KMC.Web.Models.Events;
using KMC.Web.Models.Organizer;
using KMC.Web.Models.Payments;
using KMC.Web.Models.PartnerBookings;
using KMC.Web.Models.Registrations;

namespace KMC.Web.Services;

public class KmcApiClient : IKmcApiClient
{
    private readonly HttpClient _httpClient;

    public KmcApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<EventViewModel>> GetEventsAsync(
        EventSearchViewModel filters,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            BuildEventsEndpoint(filters),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<EventViewModel>>(
                cancellationToken: cancellationToken)
            ?? [];
    }

    public async Task<IReadOnlyList<PartnerEventViewModel>>
        GetPartnerEventsAsync(
            EventSearchViewModel filters,
            CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            BuildPartnerEventsEndpoint(filters),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<PartnerEventViewModel>>(
                cancellationToken: cancellationToken)
            ?? [];
    }

    public async Task<EventViewModel?> GetEventByIdAsync(
        int eventId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/Events/{eventId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<EventViewModel>(
                cancellationToken: cancellationToken);
    }

    public async Task<AuthApiResult> RegisterAsync(
        RegisterViewModel model,
        CancellationToken cancellationToken = default)
    {
        var apiRequest = new
        {
            fullName = model.FullName.Trim(),
            email = model.Email.Trim(),
            password = model.Password,
            role = model.Role
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "api/Auth/register",
            apiRequest,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return AuthApiResult.Failure(
                await ReadErrorMessageAsync(
                    response,
                    "Registration could not be completed.",
                    cancellationToken));
        }

        var user = await response.Content
            .ReadFromJsonAsync<AuthResponseViewModel>(
                cancellationToken: cancellationToken);

        return user is null
            ? AuthApiResult.Failure(
                "The authentication response was empty.")
            : AuthApiResult.Success(user);
    }

    public async Task<AuthApiResult> LoginAsync(
        LoginViewModel model,
        CancellationToken cancellationToken = default)
    {
        var apiRequest = new
        {
            email = model.Email.Trim(),
            password = model.Password
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "api/Auth/login",
            apiRequest,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return AuthApiResult.Failure(
                await ReadErrorMessageAsync(
                    response,
                    "Login failed. Check your email and password.",
                    cancellationToken));
        }

        var user = await response.Content
            .ReadFromJsonAsync<AuthResponseViewModel>(
                cancellationToken: cancellationToken);

        return user is null
            ? AuthApiResult.Failure(
                "The authentication response was empty.")
            : AuthApiResult.Success(user);
    }

    public async Task<RegistrationApiResult> PurchaseTicketAsync(
        TicketCheckoutViewModel model,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        var apiRequest = new
        {
            ticketTierId = model.TicketTierId,
            quantity = model.Quantity,
            cardHolderName = model.CardHolderName.Trim(),
            cardNumber = model.CardNumber,
            expiryMonth = model.ExpiryMonth,
            expiryYear = model.ExpiryYear,
            cvv = model.Cvv
        };

        using var request = CreateAuthorizedRequest(
            HttpMethod.Post,
            $"api/Registrations/events/{model.EventId}",
            jwtToken);

        request.Content = JsonContent.Create(apiRequest);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            return RegistrationApiResult.Failure(
                await ReadErrorMessageAsync(
                    response,
                    "The ticket purchase could not be completed.",
                    cancellationToken));
        }

        var registration = await response.Content
            .ReadFromJsonAsync<RegistrationViewModel>(
                cancellationToken: cancellationToken);

        return registration is null
            ? RegistrationApiResult.Failure(
                "The ticket purchase response was empty.")
            : new RegistrationApiResult
            {
                IsSuccess = true,
                Message =
                    "Payment approved and ticket purchased successfully.",
                Registration = registration
            };
    }

    public async Task<IReadOnlyList<RegistrationViewModel>>
        GetMyRegistrationsAsync(
            string jwtToken,
            CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            "api/Registrations/my-registrations",
            jwtToken);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<RegistrationViewModel>>(
                cancellationToken: cancellationToken)
            ?? [];
    }

    public async Task<RegistrationApiResult> CancelRegistrationAsync(
        int registrationId,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Delete,
            $"api/Registrations/{registrationId}",
            jwtToken);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            return RegistrationApiResult.Failure(
                await ReadErrorMessageAsync(
                    response,
                    "The registration could not be cancelled.",
                    cancellationToken));
        }

        var result = await response.Content
            .ReadFromJsonAsync<CancelRegistrationResponse>(
                cancellationToken: cancellationToken);

        if (result?.Registration is null)
        {
            return RegistrationApiResult.Failure(
                "The cancellation response was empty.");
        }

        return new RegistrationApiResult
        {
            IsSuccess = true,
            Message = result.Message ??
                      "Registration cancelled successfully.",
            Registration = result.Registration
        };
    }

    public async Task<IReadOnlyList<EventViewModel>> GetMyEventsAsync(
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            "api/Events/my-events",
            jwtToken);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<EventViewModel>>(
                cancellationToken: cancellationToken)
            ?? [];
    }

    public async Task<EventApiResult> CreateEventAsync(
        EventFormViewModel model,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        return await SendEventAsync(
            HttpMethod.Post,
            "api/Events",
            model,
            jwtToken,
            "The event could not be created.",
            "Event and ticket categories created successfully.",
            cancellationToken);
    }

    public async Task<EventApiResult> UpdateEventAsync(
        int eventId,
        EventFormViewModel model,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        return await SendEventAsync(
            HttpMethod.Put,
            $"api/Events/{eventId}",
            model,
            jwtToken,
            "The event could not be updated.",
            "Event and ticket categories updated successfully.",
            cancellationToken);
    }

    public async Task<EventApiResult> DeleteEventAsync(
        int eventId,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Delete,
            $"api/Events/{eventId}",
            jwtToken);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            return EventApiResult.Failure(
                await ReadErrorMessageAsync(
                    response,
                    "The event could not be deleted.",
                    cancellationToken));
        }

        var result = await response.Content
            .ReadFromJsonAsync<DeleteEventResponse>(
                cancellationToken: cancellationToken);

        return EventApiResult.Success(
            null,
            result?.Message ?? "Event deleted successfully.");
    }

    public async Task<IReadOnlyList<EventParticipantViewModel>>
        GetEventParticipantsAsync(
            int eventId,
            string jwtToken,
            CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"api/Registrations/events/{eventId}/participants",
            jwtToken);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);
        response.EnsureSuccessStatusCode();

        var apiParticipants = await response.Content
            .ReadFromJsonAsync<List<EventParticipantApiResponse>>(
                cancellationToken: cancellationToken)
            ?? [];

        return apiParticipants
            .Select(participant => new EventParticipantViewModel
            {
                RegistrationId = participant.RegistrationId,
                EventId = eventId,
                ParticipantId = participant.ParticipantId,
                ParticipantName = participant.FullName,
                ParticipantEmail = participant.Email,
                TicketTierName = participant.TicketTierName,
                TicketPrice = participant.TicketPrice,
                Quantity = participant.Quantity,
                TotalAmount = participant.TotalAmount,
                PaymentStatus = participant.PaymentStatus,
                TransactionReference = participant.TransactionReference,
                RegisteredAt = participant.RegisteredAt,
                Status = participant.Status
            })
            .ToList();
    }


    public async Task<IReadOnlyList<PartnerEventViewModel>> GetManagedPartnerEventsAsync(
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            "api/PartnerEvents/managed",
            jwtToken);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<PartnerEventViewModel>>(
                cancellationToken: cancellationToken)
            ?? [];
    }

    public async Task<PartnerEventViewModel?> GetManagedPartnerEventByIdAsync(
        int partnerEventId,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"api/PartnerEvents/{partnerEventId}/manage",
            jwtToken);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PartnerEventViewModel>(
            cancellationToken: cancellationToken);
    }

    public async Task<PartnerEventApiResult> UpdatePartnerEventAsync(
        int partnerEventId,
        PartnerEventFormViewModel model,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Put,
            $"api/PartnerEvents/{partnerEventId}/manage",
            jwtToken);

        request.Content = JsonContent.Create(new
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
            isPublished = model.IsPublished,
            ticketTiers = model.TicketTiers
                .OrderBy(item => item.SortOrder)
                .Select(item => new
                {
                    externalTicketTierId = item.ExternalTicketTierId,
                    name = item.Name.Trim(),
                    description = item.Description?.Trim(),
                    price = item.Price,
                    capacity = item.Capacity,
                    sortOrder = item.SortOrder
                })
                .ToArray()
        });

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);
        if (!response.IsSuccessStatusCode)
        {
            return PartnerEventApiResult.Failure(
                await ReadErrorMessageAsync(
                    response,
                    "The connected event could not be updated.",
                    cancellationToken));
        }

        var eventItem = await response.Content
            .ReadFromJsonAsync<PartnerEventViewModel>(
                cancellationToken: cancellationToken);

        return PartnerEventApiResult.Success(
            eventItem,
            "Connected event and ticket categories updated successfully.");
    }

    public async Task<PartnerEventApiResult> DeletePartnerEventAsync(
        int partnerEventId,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Delete,
            $"api/PartnerEvents/{partnerEventId}/manage",
            jwtToken);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);
        if (!response.IsSuccessStatusCode)
        {
            return PartnerEventApiResult.Failure(
                await ReadErrorMessageAsync(
                    response,
                    "The connected event could not be deleted.",
                    cancellationToken));
        }

        var message = await ReadErrorMessageAsync(
            response,
            "Connected event deleted successfully.",
            cancellationToken);

        return PartnerEventApiResult.Success(null, message);
    }

    public async Task<IReadOnlyList<PartnerBookingViewModel>> GetPartnerEventBookingsAsync(
        int partnerEventId,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Get,
            $"api/PartnerEvents/{partnerEventId}/bookings",
            jwtToken);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<PartnerBookingViewModel>>(
                cancellationToken: cancellationToken)
            ?? [];
    }

    public async Task<PartnerEventViewModel?> GetPartnerEventByIdAsync(
        int partnerEventId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/PartnerEvents/{partnerEventId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PartnerEventViewModel>(
            cancellationToken: cancellationToken);
    }

    public async Task<PartnerBookingApiResult> PurchasePartnerTicketAsync(
        PartnerTicketCheckoutViewModel model,
        string jwtToken,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            model.TicketTierId,
            model.Quantity,
            model.CardHolderName,
            model.CardNumber,
            model.ExpiryMonth,
            model.ExpiryYear,
            model.Cvv
        };

        using var request = CreateAuthorizedRequest(
            HttpMethod.Post,
            $"api/PartnerEvents/{model.PartnerEventId}/purchase",
            jwtToken);

        request.Content = JsonContent.Create(body);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            return PartnerBookingApiResult.Failure(
                await ReadErrorMessageAsync(
                    response,
                    "The partner event ticket could not be purchased.",
                    cancellationToken));
        }

        var booking = await response.Content
            .ReadFromJsonAsync<PartnerBookingViewModel>(
                cancellationToken: cancellationToken);

        return booking is null
            ? PartnerBookingApiResult.Failure("The partner booking response was empty.")
            : PartnerBookingApiResult.Success(booking);
    }

    public async Task<PartnerBookingViewModel?> GetPartnerBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/PartnerEvents/bookings/{Uri.EscapeDataString(bookingReference)}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PartnerBookingViewModel>(
            cancellationToken: cancellationToken);
    }

    public async Task<PartnerBookingApiResult> CancelPartnerBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(
            $"api/PartnerEvents/bookings/{Uri.EscapeDataString(bookingReference)}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return PartnerBookingApiResult.Failure(
                await ReadErrorMessageAsync(response, "The partner booking could not be cancelled.", cancellationToken));
        }

        var booking = await response.Content.ReadFromJsonAsync<PartnerBookingViewModel>(
            cancellationToken: cancellationToken);
        return booking is null
            ? PartnerBookingApiResult.Failure("The cancellation response was empty.")
            : PartnerBookingApiResult.Success(booking);
    }

    private async Task<EventApiResult> SendEventAsync(
        HttpMethod method,
        string endpoint,
        EventFormViewModel model,
        string jwtToken,
        string fallbackError,
        string successMessage,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            method,
            endpoint,
            jwtToken);

        request.Content = JsonContent.Create(
            CreateEventRequestBody(model));

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        ThrowIfUnauthorized(response);

        if (!response.IsSuccessStatusCode)
        {
            return EventApiResult.Failure(
                await ReadErrorMessageAsync(
                    response,
                    fallbackError,
                    cancellationToken));
        }

        var eventItem = await response.Content
            .ReadFromJsonAsync<EventViewModel>(
                cancellationToken: cancellationToken);

        return eventItem is null
            ? EventApiResult.Failure(
                "The event response was empty.")
            : EventApiResult.Success(
                eventItem,
                successMessage);
    }

    private static object CreateEventRequestBody(
        EventFormViewModel model)
    {
        return new
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
            status = model.Status,
            ticketTiers = model.TicketTiers
                .OrderBy(ticketTier => ticketTier.SortOrder)
                .Select(ticketTier => new
                {
                    id = ticketTier.Id,
                    name = ticketTier.Name.Trim(),
                    price = ticketTier.Price,
                    capacity = ticketTier.Capacity,
                    sortOrder = ticketTier.SortOrder
                })
                .ToArray()
        };
    }

    private static HttpRequestMessage CreateAuthorizedRequest(
        HttpMethod method,
        string endpoint,
        string jwtToken)
    {
        var request = new HttpRequestMessage(method, endpoint);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                jwtToken);

        return request;
    }

    private static void ThrowIfUnauthorized(
        HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "The API login session has expired.");
        }
    }

    private static string BuildEventsEndpoint(
        EventSearchViewModel filters)
    {
        var parameters = new List<string>();

        AddParameter(parameters, "search", filters.Search);
        AddParameter(parameters, "eventType", filters.EventType);
        AddParameter(parameters, "venue", filters.Venue);
        AddParameter(parameters, "location", filters.Location);

        if (filters.Date.HasValue)
        {
            parameters.Add(
                $"date={filters.Date.Value:yyyy-MM-dd}");
        }

        return parameters.Count == 0
            ? "api/Events"
            : $"api/Events?{string.Join("&", parameters)}";
    }

    private static string BuildPartnerEventsEndpoint(
        EventSearchViewModel filters)
    {
        var eventsEndpoint = BuildEventsEndpoint(filters);
        return eventsEndpoint.Replace(
            "api/Events",
            "api/PartnerEvents",
            StringComparison.Ordinal);
    }

    private static void AddParameter(
        ICollection<string> parameters,
        string name,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters.Add(
                $"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }

    private static async Task<string> ReadErrorMessageAsync(
        HttpResponseMessage response,
        string fallbackMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content
                .ReadFromJsonAsync<ApiErrorResponse>(
                    cancellationToken: cancellationToken);

            if (!string.IsNullOrWhiteSpace(error?.Message))
            {
                return error.Message;
            }

            if (!string.IsNullOrWhiteSpace(error?.Detail))
            {
                return error.Detail;
            }

            var validationMessage = error?.Errors?
                .SelectMany(item => item.Value)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(validationMessage)
                ? fallbackMessage
                : validationMessage;
        }
        catch
        {
            return fallbackMessage;
        }
    }

    private sealed class EventParticipantApiResponse
    {
        public int RegistrationId { get; set; }
        public int ParticipantId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TicketTierName { get; set; } = string.Empty;
        public decimal TicketPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string TransactionReference { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private sealed class DeleteEventResponse
    {
        public string? Message { get; set; }
    }

    private sealed class CancelRegistrationResponse
    {
        public string? Message { get; set; }
        public RegistrationViewModel? Registration { get; set; }
    }

    private sealed class ApiErrorResponse
    {
        public string? Message { get; set; }
        public string? Detail { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}
