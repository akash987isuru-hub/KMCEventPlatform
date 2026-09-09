using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Participant;

[Authorize(Roles = "Participant")]
public class HomeModel : ParticipantDashboardPageModelBase
{
    public HomeModel(IKmcApiClient apiClient)
        : base(apiClient)
    {
    }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var redirect = await LoadAsync(
            "/Participant/Home",
            cancellationToken);

        return redirect ?? Page();
    }
}
