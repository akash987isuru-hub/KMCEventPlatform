using KMC.Web.Models.Events;

namespace KMC.Web.Models.Organizer;

public class EventApiResult
{
    public bool IsSuccess { get; set; }

    public string? Message { get; set; }

    public EventViewModel? Event { get; set; }

    public static EventApiResult Success(
        EventViewModel? eventItem,
        string message)
    {
        return new EventApiResult
        {
            IsSuccess = true,
            Event = eventItem,
            Message = message
        };
    }

    public static EventApiResult Failure(string message)
    {
        return new EventApiResult
        {
            IsSuccess = false,
            Message = message
        };
    }
}