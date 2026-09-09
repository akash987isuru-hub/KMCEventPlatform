using KMC.Web.Models.Events;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMC.Web.Pages.Events;

[AllowAnonymous]
public class DetailsModel : PageModel
{
    private readonly IKmcApiClient _apiClient;

    public DetailsModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public EventViewModel Event { get; private set; } = new();

    public int Id => Event.Id;
    public string Title => Event.Title;
    public string? Description => Event.Description;
    public string EventType => Event.EventType;
    public DateTime EventDate => Event.EventDate;
    public DateTime EndDate => Event.EndDate;
    public TimeSpan StartTime => Event.StartTime;
    public TimeSpan EndTime => Event.EndTime;
    public string Venue => Event.Venue;
    public string Location => Event.Location;
    public int Capacity => Event.Capacity;
    public string OrganizerName => Event.OrganizerName;
    public int RegisteredParticipantCount => Event.RegisteredParticipantCount;
    public IReadOnlyList<TicketTierViewModel> TicketTiers => Event.TicketTiers;
    public int AvailableTicketCount => CalculateAvailableTicketCount(Event);

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var eventItem = await _apiClient.GetEventByIdAsync(
                id,
                cancellationToken);

            if (eventItem is null)
            {
                return NotFound();
            }

            Event = eventItem;
            return Page();
        }
        catch (HttpRequestException)
        {
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            ErrorMessage = "The event service is currently unavailable.";
            return Page();
        }
        catch (TaskCanceledException)
        {
            Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            ErrorMessage = "The event request took too long to complete.";
            return Page();
        }
    }
    public async Task<IActionResult> OnGetAvailabilityAsync(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            var eventItem = await _apiClient.GetEventByIdAsync(
                id,
                cancellationToken);

            if (eventItem is null)
            {
                return NotFound();
            }

            var availableCount = CalculateAvailableTicketCount(eventItem);

            return new JsonResult(new
            {
                availableCount,
                isSoldOut = availableCount <= 0,
                hasEnded = eventItem.HasEnded
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
        catch (TaskCanceledException)
        {
            return StatusCode(StatusCodes.Status504GatewayTimeout);
        }
    }

    private static int CalculateAvailableTicketCount(EventViewModel eventItem)
    {
        if (eventItem.TicketTiers.Count > 0)
        {
            return eventItem.TicketTiers.Sum(
                ticketTier => Math.Max(0, ticketTier.RemainingCount));
        }

        return Math.Max(
            0,
            eventItem.Capacity - eventItem.RegisteredParticipantCount);
    }

}
