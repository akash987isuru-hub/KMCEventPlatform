namespace Organizer.Api.DTOs.Kmc;

public class KmcPublishResult
{
    public bool IsSuccess { get; init; }

    public int? KmcEventId { get; init; }

    public string? ErrorMessage { get; init; }

    public static KmcPublishResult Success(int eventId) => new()
    {
        IsSuccess = true,
        KmcEventId = eventId
    };

    public static KmcPublishResult Failure(string message) => new()
    {
        IsSuccess = false,
        ErrorMessage = message
    };
}
