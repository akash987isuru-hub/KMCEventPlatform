namespace KMC.Web.Models.Events;

public class EventSearchViewModel
{
    public string? Search { get; set; }

    public string? EventType { get; set; }

    public DateTime? Date { get; set; }

    public string? Venue { get; set; }

    public string? Location { get; set; }
}