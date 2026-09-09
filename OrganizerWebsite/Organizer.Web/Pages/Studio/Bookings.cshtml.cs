using Microsoft.AspNetCore.Mvc.RazorPages;
using Organizer.Web.Models;
using Organizer.Web.Services;

namespace Organizer.Web.Pages.Studio;

public class BookingsModel : PageModel
{
    private readonly IOrganizerApiClient _apiClient;

    public BookingsModel(IOrganizerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IReadOnlyList<OrganizerBookingViewModel> Bookings { get; private set; }
        = Array.Empty<OrganizerBookingViewModel>();
    public int TicketCount { get; private set; }
    public decimal Revenue { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Bookings = await _apiClient.GetBookingsAsync(cancellationToken: cancellationToken);
        var confirmedBookings = Bookings
            .Where(item => item.BookingStatus == "Confirmed")
            .ToList();
        TicketCount = confirmedBookings.Sum(item => item.Quantity);
        Revenue = confirmedBookings.Sum(item => item.TotalAmount);
    }
}
