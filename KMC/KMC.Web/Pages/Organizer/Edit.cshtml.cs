using KMC.Web.Pages;
using KMC.Web.Infrastructure;
using KMC.Web.Models.Organizer;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Organizer;

[Authorize(Roles = "Organizer")]
public class EditModel : AuthenticatedPageModel
{
    private readonly IKmcApiClient _apiClient;

    public EditModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public EventFormViewModel Input { get; set; } = new();

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
                Url.Page("/Organizer/Edit", new { id }) ??
                "/Organizer/Index");
        }

        try
        {
            var organizerEvents = await _apiClient.GetMyEventsAsync(
                token,
                cancellationToken);

            var eventItem = organizerEvents.FirstOrDefault(
                item => item.Id == id);

            if (eventItem is null)
            {
                return NotFound();
            }

            Input = new EventFormViewModel
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                EventType = eventItem.EventType,
                EventDate = eventItem.EventDate,
                EndDate = eventItem.EndDate,
                StartTime = eventItem.StartTime,
                EndTime = eventItem.EndTime,
                Venue = eventItem.Venue,
                Location = eventItem.Location,
                Status = eventItem.Status,
                TicketTiers = eventItem.TicketTiers
                    .OrderBy(ticketTier => ticketTier.SortOrder)
                    .Select(ticketTier => new TicketTierFormViewModel
                    {
                        Id = ticketTier.Id,
                        Name = ticketTier.Name,
                        Price = ticketTier.Price,
                        Capacity = ticketTier.Capacity,
                        SortOrder = ticketTier.SortOrder
                    })
                    .ToList()
            };

            EventFormValidator.EnsureThreeTicketTiers(Input);
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/Edit", new { id }) ??
                "/Organizer/Index");
        }
        catch (HttpRequestException)
        {
            TempData["OrganizerError"] =
                "The event service is currently unavailable.";

            return RedirectToPage("/Organizer/Index");
        }
        catch (TaskCanceledException)
        {
            TempData["OrganizerError"] =
                "The event request took too long to complete.";

            return RedirectToPage("/Organizer/Index");
        }
    }

    public async Task<IActionResult> OnPostAsync(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0 || id != Input.Id)
        {
            return BadRequest();
        }

        EventFormValidator.Validate(Input, ModelState);

        if (!ModelState.IsValid)
        {
            EventFormValidator.EnsureThreeTicketTiers(Input);
            return Page();
        }

        var token = GetJwtToken();

        if (token is null)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/Edit", new { id }) ??
                "/Organizer/Index");
        }

        try
        {
            var result = await _apiClient.UpdateEventAsync(
                id,
                Input,
                token,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message ?? "The event could not be updated.");

                return Page();
            }

            TempData["OrganizerSuccess"] =
                result.Message ?? "Event updated successfully.";

            return RedirectToPage("/Organizer/Index");
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/Edit", new { id }) ??
                "/Organizer/Index");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The event service is currently unavailable.");

            return Page();
        }
        catch (TaskCanceledException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The event request took too long to complete.");

            return Page();
        }
    }
}
