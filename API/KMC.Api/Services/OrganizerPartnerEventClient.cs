using System.Net;
using System.Net.Http.Json;
using KMC.Api.DTOs.PartnerEvents;
using KMC.Api.Entities;
using KMC.Api.Interfaces;

namespace KMC.Api.Services;

public class OrganizerPartnerEventClient : IOrganizerPartnerEventClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrganizerPartnerEventClient> _logger;

    public OrganizerPartnerEventClient(
        HttpClient httpClient,
        ILogger<OrganizerPartnerEventClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PartnerEventManagementResult> UpdateAsync(
        PartnerEvent eventItem,
        PartnerEventUpdateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            title = request.Title.Trim(),
            description = NormalizeOptional(request.Description),
            eventType = request.EventType.Trim(),
            eventDate = request.EventDate.Date,
            endDate = request.EndDate.Date,
            startTime = request.StartTime,
            endTime = request.EndTime,
            venue = request.Venue.Trim(),
            location = request.Location.Trim(),
            organizerName = eventItem.OrganizerName,
            isPublished = request.IsPublished,
            ticketTiers = request.TicketTiers
                .OrderBy(item => item.SortOrder)
                .Select(item => new
                {
                    id = item.ExternalTicketTierId,
                    name = item.Name.Trim(),
                    description = NormalizeOptional(item.Description),
                    price = item.Price,
                    capacity = item.Capacity,
                    sortOrder = item.SortOrder
                })
                .ToArray()
        };

        try
        {
            using var response = await _httpClient.PutAsJsonAsync(
                $"api/Events/{eventItem.SourceEventId}",
                body,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return PartnerEventManagementResult.Failure(
                    (int)response.StatusCode,
                    await ReadErrorAsync(
                        response,
                        "The connected organizer event could not be updated.",
                        cancellationToken));
            }

            var updatedEvent = await response.Content
                .ReadFromJsonAsync<OrganizerEventUpdateResponse>(
                    cancellationToken: cancellationToken);

            if (updatedEvent is null)
            {
                return PartnerEventManagementResult.Failure(
                    StatusCodes.Status502BadGateway,
                    "The connected organizer service returned an empty update response.");
            }

            if (!updatedEvent.IsPublishedToKmc)
            {
                return PartnerEventManagementResult.Failure(
                    StatusCodes.Status502BadGateway,
                    string.IsNullOrWhiteSpace(updatedEvent.KmcSyncError)
                        ? "The organizer event was updated, but it could not be synchronized back to KMC."
                        : updatedEvent.KmcSyncError);
            }

            return PartnerEventManagementResult.Success(
                "The connected event and ticket categories were updated successfully.");
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Connected organizer event update failed for source event {SourceEventId}.",
                eventItem.SourceEventId);

            return PartnerEventManagementResult.Failure(
                StatusCodes.Status503ServiceUnavailable,
                "The connected organizer service is currently unavailable.");
        }
    }

    public async Task<PartnerEventManagementResult> DeleteAsync(
        PartnerEvent eventItem,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.DeleteAsync(
                $"api/Events/{eventItem.SourceEventId}",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return PartnerEventManagementResult.Success(
                    "The organizer event was already removed. Its remaining KMC listing was deleted successfully.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return PartnerEventManagementResult.Failure(
                    (int)response.StatusCode,
                    await ReadErrorAsync(
                        response,
                        "The connected organizer event could not be deleted.",
                        cancellationToken));
            }

            return PartnerEventManagementResult.Success(
                "The connected event and its synchronized KMC listing were deleted successfully.");
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Connected organizer event deletion failed for source event {SourceEventId}.",
                eventItem.SourceEventId);

            return PartnerEventManagementResult.Failure(
                StatusCodes.Status503ServiceUnavailable,
                "The connected organizer service is currently unavailable.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static async Task<string> ReadErrorAsync(
        HttpResponseMessage response,
        string fallback,
        CancellationToken cancellationToken)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>(
                cancellationToken: cancellationToken);
            return string.IsNullOrWhiteSpace(error?.Message)
                ? fallback
                : error.Message;
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

    private sealed class OrganizerEventUpdateResponse
    {
        public bool IsPublishedToKmc { get; set; }
        public string? KmcSyncError { get; set; }
    }
}
