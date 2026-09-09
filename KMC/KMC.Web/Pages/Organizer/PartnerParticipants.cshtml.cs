using KMC.Web.Pages;
using KMC.Web.Models.Events;
using KMC.Web.Models.PartnerBookings;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Organizer;

[Authorize(Roles = "Organizer")]
public class PartnerParticipantsModel : AuthenticatedPageModel
{
    private readonly IKmcApiClient _apiClient;

    public PartnerParticipantsModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public PartnerEventViewModel Event { get; private set; } = new();
    public IReadOnlyList<PartnerBookingViewModel> Bookings { get; private set; }
        = Array.Empty<PartnerBookingViewModel>();

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var token = GetJwtToken();
        if (token is null)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/PartnerParticipants", new { id }) ??
                "/Organizer/Index");
        }

        try
        {
            var eventItem = await _apiClient.GetManagedPartnerEventByIdAsync(
                id,
                token,
                cancellationToken);

            if (eventItem is null)
            {
                return NotFound();
            }

            Event = eventItem;
            Bookings = await _apiClient.GetPartnerEventBookingsAsync(
                id,
                token,
                cancellationToken);
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/PartnerParticipants", new { id }) ??
                "/Organizer/Index");
        }
        catch (HttpRequestException)
        {
            TempData["OrganizerError"] =
                "The connected ticket sales service is currently unavailable.";
            return RedirectToPage("/Organizer/Index");
        }
        catch (TaskCanceledException)
        {
            TempData["OrganizerError"] =
                "The connected ticket sales request took too long to complete.";
            return RedirectToPage("/Organizer/Index");
        }
    }
}
