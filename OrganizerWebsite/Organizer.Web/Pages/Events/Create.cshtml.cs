using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Organizer.Web.Models;
using Organizer.Web.Services;

namespace Organizer.Web.Pages.Events;

public class CreateModel : PageModel
{
    private readonly IOrganizerApiClient _apiClient;

    public CreateModel(IOrganizerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public EventFormViewModel Input { get; set; } = new();

    public void OnGet()
    {
        Input.EventDate = DateTime.Today.AddDays(1);
        Input.EndDate = Input.EventDate;
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        ValidateForm();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result = await _apiClient.CreateEventAsync(Input, cancellationToken);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            TempData["SuccessMessage"] = result.Message;
            if (result.Event is { IsPublished: true, IsPublishedToKmc: false })
            {
                TempData["WarningMessage"] = result.Event.KmcSyncError ??
                    "The event is saved, but KMC synchronization is pending.";
            }

            return RedirectToPage("/Studio/Index");
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            ModelState.AddModelError(string.Empty, "Organizer API is currently unavailable.");
            return Page();
        }
    }

    private void ValidateForm()
    {
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

        if (Input.TicketTiers.Select(item => item.Name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase).Count() != Input.TicketTiers.Count)
        {
            ModelState.AddModelError("Input.TicketTiers", "Ticket category names must be different.");
        }
    }
}
