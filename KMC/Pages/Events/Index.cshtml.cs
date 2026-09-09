using KMC.Web.Models.Events;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMC.Web.Pages.Events;

[AllowAnonymous]
public class IndexModel : PageModel
{
    private readonly IKmcApiClient _apiClient;

    public IndexModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public EventSearchViewModel Filters { get; private set; } = new();
    public IReadOnlyList<EventViewModel> Events { get; private set; }
        = Array.Empty<EventViewModel>();
    public IReadOnlyList<PartnerEventViewModel> PartnerEvents { get; private set; }
        = Array.Empty<PartnerEventViewModel>();
    public string? ErrorMessage { get; private set; }
    public string? PartnerErrorMessage { get; private set; }
    public int TotalEventCount => Events.Count + PartnerEvents.Count;

    public async Task OnGetAsync(
        string? search,
        string? eventType,
        DateTime? date,
        string? venue,
        string? location,
        CancellationToken cancellationToken)
    {
        Filters = new EventSearchViewModel
        {
            Search = search,
            EventType = eventType,
            Date = date,
            Venue = venue,
            Location = location
        };

        try
        {
            Events = await _apiClient.GetEventsAsync(Filters, cancellationToken);
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "The KMC event service is currently unavailable.";
        }
        catch (TaskCanceledException)
        {
            ErrorMessage = "The KMC event request took too long to complete.";
        }

        try
        {
            PartnerEvents = await _apiClient.GetPartnerEventsAsync(
                Filters,
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            PartnerErrorMessage =
                "Partner organizer events are temporarily unavailable.";
        }
        catch (TaskCanceledException)
        {
            PartnerErrorMessage =
                "The partner event request took too long to complete.";
        }
    }
    public async Task<IActionResult> OnGetCountAsync(
        string? search,
        string? eventType,
        DateTime? date,
        string? venue,
        string? location,
        CancellationToken cancellationToken)
    {
        var filters = new EventSearchViewModel
        {
            Search = search,
            EventType = eventType,
            Date = date,
            Venue = venue,
            Location = location
        };

        try
        {
            var officialEventsTask = _apiClient.GetEventsAsync(
                filters,
                cancellationToken);
            var partnerEventsTask = _apiClient.GetPartnerEventsAsync(
                filters,
                cancellationToken);

            await Task.WhenAll(officialEventsTask, partnerEventsTask);

            var count = officialEventsTask.Result.Count +
                        partnerEventsTask.Result.Count;

            return new JsonResult(new { count });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout);
        }
    }

}
