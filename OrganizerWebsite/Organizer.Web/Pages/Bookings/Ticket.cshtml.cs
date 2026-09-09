using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Organizer.Web.Infrastructure;
using Organizer.Web.Models;
using Organizer.Web.Services;

namespace Organizer.Web.Pages.Bookings;

public class TicketModel : PageModel
{
    private readonly IOrganizerApiClient _apiClient;

    public TicketModel(IOrganizerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public OrganizerBookingViewModel Booking { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        var booking = await _apiClient.GetBookingAsync(reference, cancellationToken);
        if (booking is null)
        {
            return NotFound();
        }

        Booking = booking;
        return Page();
    }


    public async Task<IActionResult> OnGetDownloadAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        var booking = await _apiClient.GetBookingAsync(reference, cancellationToken);
        if (booking is null)
        {
            return NotFound();
        }

        if (!string.Equals(
                booking.BookingStatus,
                "Confirmed",
                StringComparison.OrdinalIgnoreCase))
        {
            TempData["WarningMessage"] = "Only confirmed tickets can be downloaded.";
            return RedirectToPage(new { reference });
        }

        var pdf = OrganizerTicketPdfGenerator.Generate(booking);
        var fileName = $"Sparkling-Events-Kandy-Ticket-{booking.BookingReference}.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    public async Task<IActionResult> OnPostCancelAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        var result = await _apiClient.CancelBookingAsync(reference, cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "WarningMessage"] =
            result.IsSuccess ? "Booking cancelled and demo payment refunded." : result.Message;
        return RedirectToPage(new { reference });
    }
}
