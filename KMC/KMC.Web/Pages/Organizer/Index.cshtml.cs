using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Organizer;

[Authorize(Roles = "Organizer")]
public class IndexModel : OrganizerDashboardPageModelBase
{
    public IndexModel(IKmcApiClient apiClient)
        : base(apiClient)
    {
    }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var redirect = await LoadDashboardAsync(
            "/Organizer/Index",
            cancellationToken);

        return redirect ?? Page();
    }

    public async Task<IActionResult> OnGetRevenueAsync(
        CancellationToken cancellationToken)
    {
        var redirect = await LoadDashboardAsync(
            "/Organizer/Index",
            cancellationToken);

        if (redirect is not null)
        {
            return Unauthorized();
        }

        Response.Headers["Cache-Control"] = "no-store, no-cache";
        return new JsonResult(new
        {
            totalRevenue = TotalRevenue,
            formattedRevenue = $"LKR {TotalRevenue:N0}",
            updatedAt = RevenueUpdatedAt.ToString("hh:mm:ss tt"),
            isComplete = string.IsNullOrWhiteSpace(RevenueErrorMessage)
        });
    }

    public async Task<IActionResult> OnPostDeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            TempData["OrganizerError"] = "Invalid event selected.";
            return RedirectToPage();
        }

        var token = GetJwtToken();

        if (token is null)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/Index") ?? "/Organizer/Index");
        }

        try
        {
            var result = await ApiClient.DeleteEventAsync(
                id,
                token,
                cancellationToken);

            TempData[result.IsSuccess
                ? "OrganizerSuccess"
                : "OrganizerError"] =
                result.Message ??
                (result.IsSuccess
                    ? "Event deleted successfully."
                    : "The event could not be deleted.");
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/Index") ?? "/Organizer/Index");
        }
        catch (HttpRequestException)
        {
            TempData["OrganizerError"] =
                "The event service is currently unavailable.";
        }
        catch (TaskCanceledException)
        {
            TempData["OrganizerError"] =
                "The delete request took too long to complete.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeletePartnerAsync(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            TempData["OrganizerError"] = "Invalid connected event selected.";
            return RedirectToPage();
        }

        var token = GetJwtToken();
        if (token is null)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/Index") ?? "/Organizer/Index");
        }

        try
        {
            var result = await ApiClient.DeletePartnerEventAsync(
                id,
                token,
                cancellationToken);

            TempData[result.IsSuccess
                ? "OrganizerSuccess"
                : "OrganizerError"] =
                result.Message ??
                (result.IsSuccess
                    ? "Connected event deleted successfully."
                    : "The connected event could not be deleted.");
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/Index") ?? "/Organizer/Index");
        }
        catch (HttpRequestException)
        {
            TempData["OrganizerError"] =
                "The connected organizer service is currently unavailable.";
        }
        catch (TaskCanceledException)
        {
            TempData["OrganizerError"] =
                "The connected event delete request took too long to complete.";
        }

        return RedirectToPage();
    }

}
