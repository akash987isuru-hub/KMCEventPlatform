using KMC.Web.Infrastructure;
using KMC.Web.Models.PartnerBookings;
using KMC.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMC.Web.Pages.PartnerEvents;

public class TicketModel : PageModel
{
    private readonly IKmcApiClient _apiClient;

    public TicketModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public PartnerBookingViewModel Booking { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        var booking = await _apiClient.GetPartnerBookingAsync(reference, cancellationToken);
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
        var booking = await _apiClient.GetPartnerBookingAsync(reference, cancellationToken);
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

        var pdf = PartnerTicketPdfGenerator.Generate(booking);
        var fileName = $"KMC-Partner-Ticket-{booking.BookingReference}.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    public async Task<IActionResult> OnPostCancelAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        var result = await _apiClient.CancelPartnerBookingAsync(reference, cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "WarningMessage"] =
            result.IsSuccess ? "Partner booking cancelled and demo payment refunded." : result.Message;
        return RedirectToPage(new { reference });
    }
}
