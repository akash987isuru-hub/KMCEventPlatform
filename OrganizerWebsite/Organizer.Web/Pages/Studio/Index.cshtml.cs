using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Organizer.Web.Models;
using Organizer.Web.Services;

namespace Organizer.Web.Pages.Studio;

public class IndexModel : PageModel
{
    private readonly IOrganizerApiClient _apiClient;

    public IndexModel(IOrganizerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IReadOnlyList<OrganizerEventViewModel> Events { get; private set; }
        = Array.Empty<OrganizerEventViewModel>();
    public IReadOnlyList<OrganizerBookingViewModel> RecentBookings { get; private set; }
        = Array.Empty<OrganizerBookingViewModel>();
    public int TicketsSold { get; private set; }
    public decimal Revenue { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        try
        {
            Events = await _apiClient.GetEventsAsync(cancellationToken);
            var bookings = await _apiClient.GetBookingsAsync(cancellationToken: cancellationToken);
            RecentBookings = bookings.Take(6).ToList();
            var approvedBookings = bookings
                .Where(item =>
                    string.Equals(item.BookingStatus, "Confirmed", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(item.PaymentStatus, "Approved", StringComparison.OrdinalIgnoreCase))
                .ToList();
            TicketsSold = approvedBookings.Sum(item => item.Quantity);
            Revenue = approvedBookings.Sum(item => item.TotalAmount);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            ErrorMessage = "Organizer API is currently unavailable.";
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _apiClient.DeleteEventAsync(id, cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "WarningMessage"] = result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostPublishAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _apiClient.PublishToKmcAsync(id, cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "WarningMessage"] = result.Message;
        return RedirectToPage();
    }
}
