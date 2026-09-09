namespace KMC.Web.Models.Events;

public class EventListViewModel
{
    public EventSearchViewModel Filters { get; set; }
        = new();

    public IReadOnlyList<EventViewModel> Events { get; set; }
        = Array.Empty<EventViewModel>();

    public string? ErrorMessage { get; set; }
}