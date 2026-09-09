using KMC.Web.Models.Events;

namespace KMC.Web.Models.Home;

public class HomeIndexViewModel
{
    public IReadOnlyList<EventViewModel> FeaturedEvents { get; set; } = [];

    public int TotalPublishedEvents { get; set; }

    public string? ErrorMessage { get; set; }
}
