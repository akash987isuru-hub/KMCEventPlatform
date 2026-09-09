using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Organizer;

[Authorize(Roles = "Organizer")]
public class HomeModel : OrganizerDashboardPageModelBase
{
    public HomeModel(IKmcApiClient apiClient)
        : base(apiClient)
    {
    }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var redirect = await LoadDashboardAsync(
            "/Organizer/Home",
            cancellationToken);

        return redirect ?? Page();
    }
}
