using System.Net;
using System.Net.Http.Json;
using KMC.Api.DTOs.PartnerEvents;
using KMC.Api.Entities;
using KMC.Api.Interfaces;

namespace KMC.Api.Services;

public class OrganizerPartnerBookingClient : IOrganizerPartnerBookingClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrganizerPartnerBookingClient> _logger;

    public OrganizerPartnerBookingClient(
        HttpClient httpClient,
        ILogger<OrganizerPartnerBookingClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PartnerBookingOperationResult> PurchaseAsync(
        PartnerEvent eventItem,
        PartnerEventTicketTier ticketTier,
        PartnerTicketPurchaseRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            ticketTierId = ticketTier.ExternalTicketTierId,
            request.CustomerName,
            request.CustomerEmail,
            request.CustomerPhone,
            request.Quantity,
            request.CardHolderName,
            request.CardNumber,
            request.ExpiryMonth,
            request.ExpiryYear,
            request.Cvv,
            bookingSource = "KMC Web"
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                $"api/Bookings/events/{Uri.EscapeDataString(eventItem.ExternalEventCode)}",
                body,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return PartnerBookingOperationResult.Failure(
                    (int)response.StatusCode,
                    await ReadErrorAsync(response, cancellationToken));
            }

            var booking = await response.Content
                .ReadFromJsonAsync<PartnerBookingResponseDto>(
                    cancellationToken: cancellationToken);

            return booking is null
                ? PartnerBookingOperationResult.Failure(
                    StatusCodes.Status502BadGateway,
                    "The organizer booking service returned an empty response.")
                : PartnerBookingOperationResult.Success(booking);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Organizer booking service is unavailable.");
            return PartnerBookingOperationResult.Failure(
                StatusCodes.Status503ServiceUnavailable,
                "The organizer booking service is currently unavailable.");
        }
    }

    public async Task<IReadOnlyList<PartnerBookingResponseDto>> GetBookingsAsync(
        int sourceEventId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/Bookings?eventId={sourceEventId}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
        return await response.Content
            .ReadFromJsonAsync<List<PartnerBookingResponseDto>>(
                cancellationToken: cancellationToken)
            ?? [];
    }

    public async Task<PartnerBookingResponseDto?> GetBookingAsync(
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
        return await response.Content.ReadFromJsonAsync<PartnerBookingResponseDto>(
            cancellationToken: cancellationToken);
    }

    public async Task<PartnerBookingOperationResult> CancelBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.DeleteAsync(
                $"api/Bookings/{Uri.EscapeDataString(bookingReference)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return PartnerBookingOperationResult.Failure(
                    (int)response.StatusCode,
                    await ReadErrorAsync(response, cancellationToken));
            }

            var booking = await response.Content.ReadFromJsonAsync<PartnerBookingResponseDto>(
                cancellationToken: cancellationToken);
            return booking is null
                ? PartnerBookingOperationResult.Failure(502, "The organizer cancellation response was empty.")
                : PartnerBookingOperationResult.Success(booking);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Organizer cancellation service is unavailable.");
            return PartnerBookingOperationResult.Failure(503, "The organizer booking service is currently unavailable.");
        }
    }

    private static async Task<string> ReadErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(
                cancellationToken: cancellationToken);
            return string.IsNullOrWhiteSpace(error?.Message)
                ? "The partner ticket purchase could not be completed."
                : error.Message;
        }
        catch
        {
            return "The partner ticket purchase could not be completed.";
        }
    }

    private sealed class ApiError
    {
        public string? Message { get; set; }
    }
}
