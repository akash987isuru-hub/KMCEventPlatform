using System.Net;
using System.Net.Http.Json;
using Organizer.Api.DTOs.Kmc;
using Organizer.Api.Entities;
using Organizer.Api.Interfaces;

namespace Organizer.Api.Services;

public class KmcPartnerEventClient : IKmcPartnerEventClient
{
    private const string ApiKeyHeaderName = "X-Partner-Api-Key";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KmcPartnerEventClient> _logger;

    public KmcPartnerEventClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<KmcPartnerEventClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<KmcPublishResult> SyncAsync(
        OrganizerEvent eventItem,
        CancellationToken cancellationToken = default)
    {
        var websiteBaseUrl = _configuration["OrganizerWebsite:BaseUrl"]
            ?? "https://localhost:7193";
        var apiKey = _configuration["KmcApi:PartnerApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return KmcPublishResult.Failure(
                "KMC partner API key is missing from Sparkling Events Kandy API configuration.");
        }

        var eventUrl = $"{websiteBaseUrl.TrimEnd('/')}/Events/Details?id={eventItem.Id}";
        var ticketTiers = eventItem.TicketTiers
            .OrderBy(item => item.SortOrder)
            .Select(item => new
            {
                externalTicketTierId = item.Id,
                item.Name,
                item.Description,
                item.Price,
                item.Capacity,
                soldCount = item.Bookings
                    .Where(booking => booking.Status == "Confirmed")
                    .Sum(booking => booking.Quantity),
                item.SortOrder
            })
            .ToList();

        var body = new
        {
            externalEventCode = eventItem.ExternalEventCode,
            sourceEventId = eventItem.Id,
            sourceSystem = "SparklingEventsKandy.Api",
            title = eventItem.Title,
            description = eventItem.Description,
            eventType = eventItem.EventType,
            eventDate = eventItem.EventDate,
            endDate = eventItem.EndDate,
            startTime = eventItem.StartTime,
            endTime = eventItem.EndTime,
            venue = eventItem.Venue,
            location = eventItem.Location,
            organizerName = eventItem.OrganizerName,
            organizerEventUrl = eventUrl,
            isPublished = eventItem.IsPublished,
            ticketTiers
        };

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/PartnerEvents/sync");
            request.Headers.Add(ApiKeyHeaderName, apiKey);
            request.Content = JsonContent.Create(body);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await SafeReadAsync(response, cancellationToken);
                return KmcPublishResult.Failure(
                    $"KMC sync failed ({(int)response.StatusCode}). {error}".Trim());
            }

            var synced = await response.Content
                .ReadFromJsonAsync<KmcPartnerEventResponseDto>(
                    cancellationToken: cancellationToken);

            return synced is null
                ? KmcPublishResult.Failure("KMC returned an empty synchronization response.")
                : KmcPublishResult.Success(synced.Id);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Could not synchronize event {EventCode} with KMC.", eventItem.ExternalEventCode);
            return KmcPublishResult.Failure("KMC.Api is currently unavailable. Use the retry button after it starts.");
        }
    }

    public async Task DeleteAsync(
        string externalEventCode,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["KmcApi:PartnerApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"api/PartnerEvents/sync/{Uri.EscapeDataString(externalEventCode)}");
            request.Headers.Add(ApiKeyHeaderName, apiKey);
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
            {
                _logger.LogWarning("KMC delete synchronization returned {StatusCode} for {EventCode}.", response.StatusCode, externalEventCode);
            }
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "KMC delete synchronization failed for {EventCode}.", externalEventCode);
        }
    }

    private static async Task<string> SafeReadAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            return string.Empty;
        }
    }
}
