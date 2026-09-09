namespace KMC.Api.DTOs.PartnerEvents;

public class PartnerBookingOperationResult
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public PartnerBookingResponseDto? Booking { get; set; }

    public static PartnerBookingOperationResult Success(
        PartnerBookingResponseDto booking) => new()
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
        Message = "Payment approved and partner event ticket booked successfully.",
        Booking = booking
    };

    public static PartnerBookingOperationResult Failure(
        int statusCode,
        string message) => new()
    {
        StatusCode = statusCode,
        Message = message
    };
}
