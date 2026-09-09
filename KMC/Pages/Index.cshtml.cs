using KMC.Web.Models.Events;
using KMC.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMC.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IKmcApiClient _apiClient;

    public IndexModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IReadOnlyList<EventViewModel> FeaturedEvents { get; private set; }
        = Array.Empty<EventViewModel>();

    public int TotalPublishedEvents { get; private set; }

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var events = await _apiClient.GetEventsAsync(
                new EventSearchViewModel(),
                cancellationToken);

            TotalPublishedEvents = events.Count;
            FeaturedEvents = events
                .OrderBy(eventItem => eventItem.EventDate)
                .ThenBy(eventItem => eventItem.StartTime)
                .Take(3)
                .ToList();
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "Live event information is temporarily unavailable.";
        }
        catch (TaskCanceledException)
        {
            ErrorMessage =
                "Live event information took too long to load.";
        }
    }
}
