namespace Organizer.Web.Models;

public class OrganizerApiResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public OrganizerEventViewModel? Event { get; init; }

    public static OrganizerApiResult Success(
        OrganizerEventViewModel? eventItem,
        string message) => new()
    {
        IsSuccess = true,
        Message = message,
        Event = eventItem
    };

    public static OrganizerApiResult Failure(string message) => new()
    {
        IsSuccess = false,
        Message = message
    };
}
