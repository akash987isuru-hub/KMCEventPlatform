using KMC.Web.Models.Events;
using KMC.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMC.Web.Pages.PartnerEvents;

public class DetailsModel : PageModel
{
    private readonly IKmcApiClient _apiClient;

    public DetailsModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public PartnerEventViewModel EventItem { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var eventItem = await _apiClient.GetPartnerEventByIdAsync(id, cancellationToken);
        if (eventItem is null)
        {
            return NotFound();
        }

        EventItem = eventItem;
        return Page();
    }
}
