using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Participant;

[Authorize(Roles = "Participant")]
public class IndexModel : ParticipantDashboardPageModelBase
{
    public IndexModel(IKmcApiClient apiClient)
        : base(apiClient)
    {
    }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var redirect = await LoadAsync(
            "/Participant/Index",
            cancellationToken);

        return redirect ?? Page();
    }
}
