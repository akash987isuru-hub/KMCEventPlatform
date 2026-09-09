using KMC.Web.Pages;
using KMC.Web.Infrastructure;
using KMC.Web.Models.Organizer;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Organizer;

[Authorize(Roles = "Organizer")]
public class CreateModel : AuthenticatedPageModel
{
    private readonly IKmcApiClient _apiClient;

    public CreateModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public EventFormViewModel Input { get; set; } = new();

    public void OnGet()
    {
        Input = new EventFormViewModel
        {
            EventDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(1),
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(11, 0, 0),
            Status = "Published",
            TicketTiers = EventFormViewModel.CreateDefaultTicketTiers()
        };
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
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
                Url.Page("/Organizer/Create") ?? "/Organizer/Create");
        }

        try
        {
            var result = await _apiClient.CreateEventAsync(
                Input,
                token,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message ?? "The event could not be created.");

                return Page();
            }

            TempData["OrganizerSuccess"] =
                result.Message ?? "Event created successfully.";

            return RedirectToPage("/Organizer/Index");
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Organizer/Create") ?? "/Organizer/Create");
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
