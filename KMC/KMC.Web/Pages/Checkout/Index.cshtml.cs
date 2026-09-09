using KMC.Web.Pages;
using KMC.Web.Infrastructure;
using KMC.Web.Models.Events;
using KMC.Web.Models.Payments;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Checkout;

[Authorize(Roles = "Participant")]
public class IndexModel : AuthenticatedPageModel
{
    private readonly IKmcApiClient _apiClient;

    public IndexModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public TicketCheckoutViewModel Input { get; set; } = new();

    public EventViewModel? Event => Input.Event;

    public TicketTierViewModel? SelectedTicket => Input.SelectedTicket;

    public async Task<IActionResult> OnGetAsync(
        int eventId,
        int ticketTierId,
        CancellationToken cancellationToken)
    {
        if (eventId <= 0 || ticketTierId <= 0)
        {
            return BadRequest();
        }

        Input = new TicketCheckoutViewModel
        {
            EventId = eventId,
            TicketTierId = ticketTierId,
            Quantity = 1,
            ExpiryMonth = DateTime.Today.Month,
            ExpiryYear = DateTime.Today.Year + 1
        };

        return await LoadCheckoutViewAsync(Input, cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        EventViewModel? eventItem;

        try
        {
            eventItem = await _apiClient.GetEventByIdAsync(
                Input.EventId,
                cancellationToken);
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

        if (eventItem is null)
        {
            return NotFound();
        }

        var selectedTicket = eventItem.TicketTiers.FirstOrDefault(
            ticketTier => ticketTier.Id == Input.TicketTierId);

        if (selectedTicket is null)
        {
            ModelState.AddModelError(
                "Input.TicketTierId",
                "Select a valid ticket category.");
        }
        else if (selectedTicket.RemainingCount <= 0)
        {
            ModelState.AddModelError(
                "Input.TicketTierId",
                "The selected ticket category is sold out.");
        }
        else if (Input.Quantity > selectedTicket.RemainingCount)
        {
            ModelState.AddModelError(
                "Input.Quantity",
                $"Only {selectedTicket.RemainingCount} ticket(s) are available.");
        }

        if (eventItem.HasEnded)
        {
            ModelState.AddModelError(
                string.Empty,
                "This event has ended and tickets can no longer be purchased.");
        }

        Input.Event = eventItem;
        Input.SelectedTicket = selectedTicket;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = GetJwtToken();

        if (token is null)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Events/Details", new { id = Input.EventId }) ??
                "/Events/Index");
        }

        try
        {
            var result = await _apiClient.PurchaseTicketAsync(
                Input,
                token,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.Message ??
                    "The ticket purchase could not be completed.");

                return Page();
            }

            TempData["RegistrationSuccess"] =
                result.Message ??
                "Payment approved and ticket purchased successfully.";

            if (result.Registration is not null)
            {
                return RedirectToPage(
                    "/Registrations/PrintTicket",
                    new { registrationId = result.Registration.Id });
            }

            return RedirectToPage("/Registrations/Index");
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Events/Details", new { id = Input.EventId }) ??
                "/Events/Index");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The payment service is currently unavailable.");

            return Page();
        }
        catch (TaskCanceledException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The payment request took too long to complete.");

            return Page();
        }
    }

    private async Task<IActionResult> LoadCheckoutViewAsync(
        TicketCheckoutViewModel model,
        CancellationToken cancellationToken)
    {
        try
        {
            var eventItem = await _apiClient.GetEventByIdAsync(
                model.EventId,
                cancellationToken);

            if (eventItem is null)
            {
                return NotFound();
            }

            var ticket = eventItem.TicketTiers.FirstOrDefault(
                ticketTier => ticketTier.Id == model.TicketTierId);

            if (ticket is null)
            {
                return NotFound();
            }

            if (eventItem.HasEnded)
            {
                TempData["RegistrationError"] =
                    "This event has ended and tickets can no longer be purchased.";

                return RedirectToPage(
                    "/Events/Details",
                    new { id = model.EventId });
            }

            if (ticket.RemainingCount <= 0)
            {
                TempData["RegistrationError"] =
                    "The selected ticket category is sold out.";

                return RedirectToPage(
                    "/Events/Details",
                    new { id = model.EventId });
            }

            model.Event = eventItem;
            model.SelectedTicket = ticket;

            return Page();
        }
        catch (HttpRequestException)
        {
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            ModelState.AddModelError(
                string.Empty,
                "The event service is currently unavailable.");
            return Page();
        }
    }
}
