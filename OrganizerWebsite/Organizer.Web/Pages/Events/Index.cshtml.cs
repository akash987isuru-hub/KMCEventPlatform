using Microsoft.AspNetCore.Mvc.RazorPages;
using Organizer.Web.Models;
using Organizer.Web.Services;

namespace Organizer.Web.Pages.Events;

public class IndexModel : PageModel
{
    private readonly IOrganizerApiClient _apiClient;

    public IndexModel(IOrganizerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IReadOnlyList<OrganizerEventViewModel> Events { get; private set; }
        = Array.Empty<OrganizerEventViewModel>();
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Events = (await _apiClient.GetEventsAsync(cancellationToken))
                .Where(item => item.IsPublished)
                .OrderBy(item => item.HasEnded)
                .ThenBy(item => item.EventDate)
                .ThenBy(item => item.StartTime)
                .ToList();
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = "Events are temporarily unavailable.";
        }
    }
}
