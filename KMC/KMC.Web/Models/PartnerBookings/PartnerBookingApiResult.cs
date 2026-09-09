namespace KMC.Web.Models.PartnerBookings;

public class PartnerBookingApiResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public PartnerBookingViewModel? Booking { get; init; }

    public static PartnerBookingApiResult Success(PartnerBookingViewModel booking) => new()
    {
        IsSuccess = true,
        Message = "Payment approved and partner event ticket booked successfully.",
        Booking = booking
    };

    public static PartnerBookingApiResult Failure(string message) => new()
    {
        Message = message
    };
}
