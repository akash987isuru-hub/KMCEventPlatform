using KMC.Web.Pages;
using KMC.Web.Models.Events;
using KMC.Web.Models.Registrations;
using KMC.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Participant;

public abstract class ParticipantDashboardPageModelBase :
    AuthenticatedPageModel
{
    private readonly IKmcApiClient _apiClient;

    protected ParticipantDashboardPageModelBase(
        IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IReadOnlyList<RegistrationViewModel> Registrations
        { get; protected set; } = Array.Empty<RegistrationViewModel>();

    public IReadOnlyList<EventViewModel> RecommendedEvents
        { get; protected set; } = Array.Empty<EventViewModel>();

    public int TotalTickets => Registrations.Sum(registration => Math.Max(1, registration.Quantity));

    public int ConfirmedTickets => Registrations
        .Where(registration => string.Equals(
            registration.Status,
            "Confirmed",
            StringComparison.OrdinalIgnoreCase))
        .Sum(registration => Math.Max(1, registration.Quantity));

    public int UpcomingEvents => Registrations.Count(registration =>
        string.Equals(
            registration.Status,
            "Confirmed",
            StringComparison.OrdinalIgnoreCase) &&
        registration.EventDate.Date.Add(registration.StartTime) > DateTime.Now);

    public decimal TotalPaid => Registrations
        .Where(registration =>
            string.Equals(
                registration.PaymentStatus,
                "Approved",
                StringComparison.OrdinalIgnoreCase))
        .Sum(registration => registration.TotalAmount);

    public string? ErrorMessage { get; protected set; }

    protected async Task<IActionResult?> LoadAsync(
        string returnPage,
        CancellationToken cancellationToken)
    {
        var token = GetJwtToken();

        if (token is null)
        {
            return await RedirectToLoginAsync(
                Url.Page(returnPage) ?? returnPage);
        }

        try
        {
            var registrations = await _apiClient.GetMyRegistrationsAsync(
                token,
                cancellationToken);

            Registrations = registrations
                .Where(registration => !string.Equals(
                    registration.Status,
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        registration.Status,
                        "Canceled",
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(registration => registration.EventDate)
                .ThenBy(registration => registration.StartTime)
                .ToList();

            RecommendedEvents =
                (await _apiClient.GetEventsAsync(
                    new EventSearchViewModel(),
                    cancellationToken))
                .Where(eventItem =>
                    Registrations.All(registration =>
                        registration.EventId != eventItem.Id ||
                        !string.Equals(
                            registration.Status,
                            "Confirmed",
                            StringComparison.OrdinalIgnoreCase)))
                .OrderBy(eventItem => eventItem.EventDate)
                .ThenBy(eventItem => eventItem.StartTime)
                .Take(6)
                .ToList();
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page(returnPage) ?? returnPage);
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "The public user service is currently unavailable.";
        }
        catch (TaskCanceledException)
        {
            ErrorMessage =
                "The public user request took too long to complete.";
        }

        return null;
    }
}
