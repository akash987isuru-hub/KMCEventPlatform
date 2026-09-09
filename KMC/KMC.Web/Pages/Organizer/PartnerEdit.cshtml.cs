using KMC.Web.Pages;
using KMC.Web.Models.Organizer;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Organizer;

[Authorize(Roles = "Organizer")]
public class PartnerEditModel : AuthenticatedPageModel
{
    private readonly IKmcApiClient _apiClient;

    public PartnerEditModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public PartnerEventFormViewModel Input { get; set; } = new();

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
                Url.Page("/Organizer/PartnerEdit", new { id }) ??
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

            Input = new PartnerEventFormViewModel
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
                IsPublished = eventItem.IsPublished,
                TicketTiers = eventItem.TicketTiers
                    .OrderBy(item => item.SortOrder)
                    .Select(item => new PartnerTicketTierFormViewModel
                    {
                        ExternalTicketTierId = item.ExternalTicketTierId,
                        Name = item.Name,
                        Description = item.Description,
                        Price = item.Price,
                        Capacity = item.Capacity,
                        SoldCount = item.SoldCount,
                        SortOrder = item.SortOrder
                    })
                    .ToList()
            };

            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/PartnerEdit", new { id }) ??
                "/Organizer/Index");
        }
        catch (HttpRequestException)
        {
            TempData["OrganizerError"] =
                "The connected organizer service is currently unavailable.";
            return RedirectToPage("/Organizer/Index");
        }
        catch (TaskCanceledException)
        {
            TempData["OrganizerError"] =
                "The connected event request took too long to complete.";
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

        ValidateInput();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = GetJwtToken();
        if (token is null)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/PartnerEdit", new { id }) ??
                "/Organizer/Index");
        }

        try
        {
            var result = await _apiClient.UpdatePartnerEventAsync(
                id,
                Input,
                token,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message ?? "The connected event could not be updated.");
                return Page();
            }

            TempData["OrganizerSuccess"] =
                result.Message ?? "Connected event updated successfully.";
            return RedirectToPage("/Organizer/Index");
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/PartnerEdit", new { id }) ??
                "/Organizer/Index");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The connected organizer service is currently unavailable.");
            return Page();
        }
        catch (TaskCanceledException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The connected event update took too long to complete.");
            return Page();
        }
    }

    private void ValidateInput()
    {
        var start = Input.EventDate.Date.Add(Input.StartTime);
        var end = Input.EndDate.Date.Add(Input.EndTime);
        if (end <= start)
        {
            ModelState.AddModelError(
                "Input.EndTime",
                "Event end date and time must be later than the start date and time.");
        }

        if (Input.TicketTiers.Count is < 1 or > 5)
        {
            ModelState.AddModelError(
                "Input.TicketTiers",
                "Add between one and five ticket categories.");
        }

        if (Input.TicketTiers
            .Select(item => item.Name?.Trim())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() != Input.TicketTiers.Count)
        {
            ModelState.AddModelError(
                "Input.TicketTiers",
                "Ticket category names must be unique.");
        }

        if (Input.TicketTiers.Select(item => item.SortOrder).Distinct().Count() !=
            Input.TicketTiers.Count)
        {
            ModelState.AddModelError(
                "Input.TicketTiers",
                "Ticket category order values must be unique.");
        }

        for (var index = 0; index < Input.TicketTiers.Count; index++)
        {
            var tier = Input.TicketTiers[index];
            if (tier.Capacity < tier.SoldCount)
            {
                ModelState.AddModelError(
                    $"Input.TicketTiers[{index}].Capacity",
                    $"Capacity cannot be lower than the {tier.SoldCount} ticket(s) already sold.");
            }
        }
    }
}
