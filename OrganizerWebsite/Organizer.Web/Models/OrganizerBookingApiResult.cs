namespace Organizer.Web.Models;

public class OrganizerBookingApiResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public OrganizerBookingViewModel? Booking { get; init; }

    public static OrganizerBookingApiResult Success(OrganizerBookingViewModel booking) => new()
    {
        IsSuccess = true,
        Message = "Payment approved. Your ticket is ready.",
        Booking = booking
    };

    public static OrganizerBookingApiResult Failure(string message) => new()
    {
        Message = message
    };
}
