namespace KMC.Api.DTOs.PartnerEvents;

public class PartnerBookingListResult
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<PartnerBookingResponseDto> Bookings { get; set; }
        = Array.Empty<PartnerBookingResponseDto>();

    public static PartnerBookingListResult Success(
        IReadOnlyList<PartnerBookingResponseDto> bookings) => new()
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
        Bookings = bookings
    };

    public static PartnerBookingListResult Failure(
        int statusCode,
        string message) => new()
    {
        StatusCode = statusCode,
        Message = message
    };
}
