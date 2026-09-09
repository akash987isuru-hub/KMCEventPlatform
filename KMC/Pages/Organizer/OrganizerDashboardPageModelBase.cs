using KMC.Web.Models.Events;
using KMC.Web.Pages;
using KMC.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Organizer;

public abstract class OrganizerDashboardPageModelBase : AuthenticatedPageModel
{
    private readonly IKmcApiClient _apiClient;

    protected OrganizerDashboardPageModelBase(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    protected IKmcApiClient ApiClient => _apiClient;

    public IReadOnlyList<EventViewModel> Events { get; protected set; } = [];
    public IReadOnlyList<PartnerEventViewModel> PartnerEvents { get; protected set; } = [];

    public int TotalEvents => Events.Count + PartnerEvents.Count;
    public int PublishedEvents =>
        Events.Count(item => string.Equals(item.Status, "Published", StringComparison.OrdinalIgnoreCase)) +
        PartnerEvents.Count(item => item.IsPublished);
    public int TotalRegistrations =>
        Events.Sum(item => item.RegisteredParticipantCount) +
        PartnerEvents.Sum(item => item.RegisteredParticipantCount);
    public decimal TotalRevenue { get; protected set; }
    public DateTime RevenueUpdatedAt { get; protected set; }

    public string? ErrorMessage { get; protected set; }
    public string? PartnerErrorMessage { get; protected set; }
    public string? RevenueErrorMessage { get; protected set; }

    protected async Task<IActionResult?> LoadDashboardAsync(
        string returnPage,
        CancellationToken cancellationToken)
    {
        var token = GetJwtToken();
        if (token is null)
        {
            return await RedirectToLoginAsync(Url.Page(returnPage) ?? returnPage);
        }

        try
        {
            Events = await _apiClient.GetMyEventsAsync(token, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(Url.Page(returnPage) ?? returnPage);
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "The event service is currently unavailable.";
        }
        catch (TaskCanceledException)
        {
            ErrorMessage = "The event request took too long to complete.";
        }

        try
        {
            PartnerEvents = await _apiClient.GetManagedPartnerEventsAsync(
                token,
                cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(Url.Page(returnPage) ?? returnPage);
        }
        catch (HttpRequestException)
        {
            PartnerErrorMessage = "Events from the matching personal organizer website are temporarily unavailable.";
        }
        catch (TaskCanceledException)
        {
            PartnerErrorMessage = "The personal website event request took too long to complete.";
        }

        await LoadRevenueAsync(token, cancellationToken);
        return null;
    }

    private async Task LoadRevenueAsync(
        string token,
        CancellationToken cancellationToken)
    {
        decimal totalRevenue = 0;
        var revenueIncomplete = false;

        foreach (var eventItem in Events)
        {
            try
            {
                var participants = await _apiClient.GetEventParticipantsAsync(
                    eventItem.Id,
                    token,
                    cancellationToken);

                totalRevenue += participants
                    .Where(item =>
                        string.Equals(item.Status, "Confirmed", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(item.PaymentStatus, "Approved", StringComparison.OrdinalIgnoreCase))
                    .Sum(item => item.TotalAmount);
            }
            catch (Exception exception) when (
                exception is HttpRequestException or TaskCanceledException or UnauthorizedAccessException)
            {
                revenueIncomplete = true;
            }
        }

        foreach (var partnerEvent in PartnerEvents)
        {
            try
            {
                var bookings = await _apiClient.GetPartnerEventBookingsAsync(
                    partnerEvent.Id,
                    token,
                    cancellationToken);

                totalRevenue += bookings
                    .Where(item =>
                        string.Equals(item.BookingStatus, "Confirmed", StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(item.PaymentStatus, "Approved", StringComparison.OrdinalIgnoreCase))
                    .Sum(item => item.TotalAmount);
            }
            catch (Exception exception) when (
                exception is HttpRequestException or TaskCanceledException or UnauthorizedAccessException)
            {
                revenueIncomplete = true;
            }
        }

        TotalRevenue = totalRevenue;
        RevenueUpdatedAt = DateTime.Now;
        RevenueErrorMessage = revenueIncomplete
            ? "Revenue is temporarily showing the payments that could be verified."
            : null;
    }
}
