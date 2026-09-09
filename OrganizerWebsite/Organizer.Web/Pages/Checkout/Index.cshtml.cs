using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Organizer.Web.Models;
using Organizer.Web.Services;

namespace Organizer.Web.Pages.Checkout;

public class IndexModel : PageModel
{
    private readonly IOrganizerApiClient _apiClient;

    public IndexModel(IOrganizerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public OrganizerCheckoutViewModel Input { get; set; } = new();

    public OrganizerEventViewModel EventItem { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(
        int eventId,
        int? ticketTierId,
        CancellationToken cancellationToken)
    {
        var eventItem = await _apiClient.GetEventAsync(eventId, cancellationToken);
        if (eventItem is null || !eventItem.IsPublished)
        {
            return NotFound();
        }

        if (eventItem.HasEnded || eventItem.IsSoldOut)
        {
            return RedirectToPage("/Events/Details", new { id = eventId });
        }

        EventItem = eventItem;
        Input.EventId = eventItem.Id;
        Input.ExternalEventCode = eventItem.ExternalEventCode;
        Input.TicketTierId = ticketTierId ??
            eventItem.TicketTiers.FirstOrDefault(item => item.AvailableCount > 0)?.Id ?? 0;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        var eventItem = await _apiClient.GetEventAsync(Input.EventId, cancellationToken);
        if (eventItem is null)
        {
            return NotFound();
        }

        EventItem = eventItem;
        Input.ExternalEventCode = eventItem.ExternalEventCode;

        var selectedTier = eventItem.TicketTiers.FirstOrDefault(item => item.Id == Input.TicketTierId);
        if (eventItem.HasEnded)
        {
            ModelState.AddModelError(string.Empty, "This event has ended and ticket sales are closed.");
        }
        else if (selectedTier is null)
        {
            ModelState.AddModelError("Input.TicketTierId", "Select a valid ticket category.");
        }
        else if (selectedTier.AvailableCount <= 0)
        {
            ModelState.AddModelError("Input.TicketTierId", "This ticket category is sold out.");
        }
        else if (Input.Quantity > selectedTier.AvailableCount)
        {
            ModelState.AddModelError("Input.Quantity", $"Only {selectedTier.AvailableCount} ticket(s) are available.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _apiClient.PurchaseTicketAsync(Input, cancellationToken);
        if (!result.IsSuccess || result.Booking is null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return Page();
        }

        return RedirectToPage(
            "/Bookings/Ticket",
            new { reference = result.Booking.BookingReference });
    }
}
