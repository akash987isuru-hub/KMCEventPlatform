namespace KMC.Api.DTOs.PartnerEvents;

public class PartnerEventManagementResult
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public PartnerEventResponseDto? Event { get; set; }

    public static PartnerEventManagementResult Success(
        PartnerEventResponseDto eventItem,
        string message) => new()
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
        Message = message,
        Event = eventItem
    };

    public static PartnerEventManagementResult Success(
        string message) => new()
    {
        IsSuccess = true,
        StatusCode = StatusCodes.Status200OK,
        Message = message
    };

    public static PartnerEventManagementResult Failure(
        int statusCode,
        string message) => new()
    {
        StatusCode = statusCode,
        Message = message
    };
}
