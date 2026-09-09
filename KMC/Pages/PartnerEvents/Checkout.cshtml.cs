using KMC.Web.Pages;
using KMC.Web.Models.Events;
using KMC.Web.Models.PartnerBookings;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.PartnerEvents;

[Authorize(Roles = "Participant")]
public class CheckoutModel : AuthenticatedPageModel
{
    private readonly IKmcApiClient _apiClient;

    public CheckoutModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public PartnerTicketCheckoutViewModel Input { get; set; } = new();

    public PartnerEventViewModel EventItem { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(
        int eventId,
        int ticketTierId,
        CancellationToken cancellationToken)
    {
        if (eventId <= 0 || ticketTierId <= 0)
        {
            return BadRequest();
        }

        var eventItem = await _apiClient.GetPartnerEventByIdAsync(
            eventId,
            cancellationToken);

        if (eventItem is null)
        {
            return NotFound();
        }

        EventItem = eventItem;

        if (eventItem.HasEnded || eventItem.IsSoldOut)
        {
            return RedirectToPage(
                "/PartnerEvents/Details",
                new { id = eventItem.Id });
        }

        var selectedTier = eventItem.TicketTiers.FirstOrDefault(
            item => item.Id == ticketTierId && item.AvailableCount > 0);

        if (selectedTier is null)
        {
            return RedirectToPage(
                "/PartnerEvents/Details",
                new { id = eventItem.Id });
        }

        Input = new PartnerTicketCheckoutViewModel
        {
            PartnerEventId = eventItem.Id,
            TicketTierId = selectedTier.Id,
            Quantity = 1,
            ExpiryMonth = DateTime.Today.Month,
            ExpiryYear = DateTime.Today.Year + 1
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        var eventItem = await _apiClient.GetPartnerEventByIdAsync(
            Input.PartnerEventId,
            cancellationToken);

        if (eventItem is null)
        {
            return NotFound();
        }

        EventItem = eventItem;

        if (eventItem.HasEnded)
        {
            ModelState.AddModelError(
                string.Empty,
                "This event has ended and ticket purchasing is closed.");
        }

        var selectedTier = eventItem.TicketTiers
            .FirstOrDefault(item => item.Id == Input.TicketTierId);

        if (selectedTier is null)
        {
            ModelState.AddModelError(
                "Input.TicketTierId",
                "The selected ticket category is unavailable.");
        }
        else if (selectedTier.AvailableCount <= 0)
        {
            ModelState.AddModelError(
                "Input.TicketTierId",
                "The selected ticket category is sold out.");
        }
        else if (Input.Quantity > selectedTier.AvailableCount)
        {
            ModelState.AddModelError(
                "Input.Quantity",
                $"Only {selectedTier.AvailableCount} ticket(s) are currently available.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = GetJwtToken();
        if (token is null)
        {
            return await RedirectToLoginAsync(
                BuildCheckoutReturnUrl());
        }

        try
        {
            var result = await _apiClient.PurchasePartnerTicketAsync(
                Input,
                token,
                cancellationToken);

            if (!result.IsSuccess || result.Booking is null)
            {
                ModelState.AddModelError(string.Empty, result.Message);
                return Page();
            }

            return RedirectToPage(
                "/PartnerEvents/Ticket",
                new { reference = result.Booking.BookingReference });
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                BuildCheckoutReturnUrl());
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The connected payment service is currently unavailable.");
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

    private string BuildCheckoutReturnUrl() =>
        Url.Page(
            "/PartnerEvents/Checkout",
            new
            {
                eventId = Input.PartnerEventId,
                ticketTierId = Input.TicketTierId
            }) ?? "/Events";
}
