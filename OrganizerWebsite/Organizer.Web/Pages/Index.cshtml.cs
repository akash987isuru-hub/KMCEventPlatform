using Microsoft.AspNetCore.Mvc.RazorPages;
using Organizer.Web.Models;
using Organizer.Web.Services;

namespace Organizer.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IOrganizerApiClient _apiClient;

    public IndexModel(IOrganizerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IReadOnlyList<OrganizerEventViewModel> FeaturedEvents { get; private set; }
        = Array.Empty<OrganizerEventViewModel>();
    public int EventCount { get; private set; }
    public int TicketCount { get; private set; }
    public int KmcPublishedCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            var events = await _apiClient.GetEventsAsync(cancellationToken);
            EventCount = events.Count;
            TicketCount = events.Sum(item => item.SoldCount);
            KmcPublishedCount = events.Count(item => item.IsPublishedToKmc);
            FeaturedEvents = events
                .Where(item => item.IsPublished && !item.HasEnded)
                .OrderBy(item => item.EventDate)
                .Take(3)
                .ToList();
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = "Our event service is taking a short break. Please try again in a moment.";
        }
    }
}
