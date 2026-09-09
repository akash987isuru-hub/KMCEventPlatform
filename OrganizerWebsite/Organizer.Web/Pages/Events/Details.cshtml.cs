using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Organizer.Web.Models;
using Organizer.Web.Services;

namespace Organizer.Web.Pages.Events;

public class DetailsModel : PageModel
{
    private readonly IOrganizerApiClient _apiClient;

    public DetailsModel(IOrganizerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public OrganizerEventViewModel EventItem { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var eventItem = await _apiClient.GetEventAsync(id, cancellationToken);
        if (eventItem is null || !eventItem.IsPublished)
        {
            return NotFound();
        }

        EventItem = eventItem;
        return Page();
    }
}
