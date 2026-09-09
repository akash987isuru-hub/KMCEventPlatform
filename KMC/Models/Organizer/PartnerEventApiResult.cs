using KMC.Web.Models.Events;

namespace KMC.Web.Models.Organizer;

public class PartnerEventApiResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public PartnerEventViewModel? Event { get; set; }

    public static PartnerEventApiResult Success(
        PartnerEventViewModel? eventItem,
        string message) => new()
    {
        IsSuccess = true,
        Event = eventItem,
        Message = message
    };

    public static PartnerEventApiResult Failure(string message) => new()
    {
        Message = message
    };
}
