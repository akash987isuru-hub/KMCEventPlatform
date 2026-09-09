using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Organizer.Web.Models;
using Organizer.Web.Services;

namespace Organizer.Web.Pages.Events;

public class EditModel : PageModel
{
    private readonly IOrganizerApiClient _apiClient;

    public EditModel(IOrganizerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public EventFormViewModel Input { get; set; } = new();
    public int EventId { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await _apiClient.GetEventAsync(id, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        EventId = id;
        Input = new EventFormViewModel
        {
            Title = item.Title,
            Description = item.Description,
            EventType = item.EventType,
            EventDate = item.EventDate,
            EndDate = item.EndDate,
            StartTime = item.StartTime,
            EndTime = item.EndTime,
            Venue = item.Venue,
            Location = item.Location,
            OrganizerName = item.OrganizerName,
            IsPublished = item.IsPublished,
            TicketTiers = item.TicketTiers.Count == 0
                ? EventFormViewModel.CreateDefaultTiers()
                : item.TicketTiers
                    .OrderBy(tier => tier.SortOrder)
                    .Select(tier => new TicketTierFormViewModel
                    {
                        Id = tier.Id,
                        Name = tier.Name,
                        Description = tier.Description,
                        Price = tier.Price,
                        Capacity = tier.Capacity,
                        SortOrder = tier.SortOrder
                    })
                    .ToList()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        int id,
        CancellationToken cancellationToken)
    {
        EventId = id;
        if (Input.EventDate.Date < DateTime.Today)
        {
            ModelState.AddModelError("Input.EventDate", "Event date cannot be in the past.");
        }

        var start = Input.EventDate.Date.Add(Input.StartTime);
        var end = Input.EndDate.Date.Add(Input.EndTime);
        if (end <= start)
        {
            ModelState.AddModelError("Input.EndDate", "End date and time must be later than the start date and time.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _apiClient.UpdateEventAsync(id, Input, cancellationToken);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        TempData["SuccessMessage"] = result.Message;
        if (result.Event is { IsPublished: true, IsPublishedToKmc: false })
        {
            TempData["WarningMessage"] = result.Event.KmcSyncError;
        }

        return RedirectToPage("/Studio/Index");
    }
}
