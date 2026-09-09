using KMC.Web.Pages;
using KMC.Web.Models.Organizer;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Organizer;

[Authorize(Roles = "Organizer")]
public class ParticipantsModel : AuthenticatedPageModel
{
    private readonly IKmcApiClient _apiClient;

    public ParticipantsModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IReadOnlyList<EventParticipantViewModel> Participants
        { get; private set; } = Array.Empty<EventParticipantViewModel>();

    public int EventId { get; private set; }

    public string EventTitle { get; private set; } = string.Empty;

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
                Url.Page("/Organizer/Participants", new { id }) ??
                "/Organizer/Index");
        }

        try
        {
            var events = await _apiClient.GetMyEventsAsync(
                token,
                cancellationToken);

            var eventItem = events.FirstOrDefault(item => item.Id == id);

            if (eventItem is null)
            {
                return NotFound();
            }

            Participants = await _apiClient.GetEventParticipantsAsync(
                id,
                token,
                cancellationToken);

            EventId = eventItem.Id;
            EventTitle = eventItem.Title;
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/Participants", new { id }) ??
                "/Organizer/Index");
        }
        catch (HttpRequestException)
        {
            TempData["OrganizerError"] =
                "The participant service is currently unavailable.";

            return RedirectToPage("/Organizer/Index");
        }
        catch (TaskCanceledException)
        {
            TempData["OrganizerError"] =
                "The participant request took too long to complete.";

            return RedirectToPage("/Organizer/Index");
        }
    }
}
