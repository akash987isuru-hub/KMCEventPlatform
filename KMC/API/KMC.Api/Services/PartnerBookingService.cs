using KMC.Api.Data;
using KMC.Api.DTOs.PartnerEvents;
using KMC.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KMC.Api.Services;

public class PartnerBookingService : IPartnerBookingService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IOrganizerPartnerBookingClient _bookingClient;

    public PartnerBookingService(
        ApplicationDbContext dbContext,
        IOrganizerPartnerBookingClient bookingClient)
    {
        _dbContext = dbContext;
        _bookingClient = bookingClient;
    }

    public async Task<PartnerBookingOperationResult> PurchaseAsync(
        int partnerEventId,
        PartnerTicketPurchaseRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var eventItem = await _dbContext.PartnerEvents
            .Include(item => item.TicketTiers)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == partnerEventId && item.IsPublished,
                cancellationToken);

        if (eventItem is null)
        {
            return PartnerBookingOperationResult.Failure(
                StatusCodes.Status404NotFound,
                "Partner event was not found.");
        }

        var eventEnd = eventItem.EndDate.Date.Add(eventItem.EndTime);
        if (eventEnd <= KMC.Api.Infrastructure.KmcTime.Now)
        {
            return PartnerBookingOperationResult.Failure(
                StatusCodes.Status400BadRequest,
                "This partner event has ended.");
        }

        var ticketTier = eventItem.TicketTiers
            .FirstOrDefault(item => item.Id == request.TicketTierId);

        if (ticketTier is null)
        {
            return PartnerBookingOperationResult.Failure(
                StatusCodes.Status404NotFound,
                "Ticket category was not found.");
        }

        if (request.Quantity > ticketTier.Capacity - ticketTier.SoldCount)
        {
            return PartnerBookingOperationResult.Failure(
                StatusCodes.Status400BadRequest,
                "The selected ticket quantity is no longer available.");
        }

        return await _bookingClient.PurchaseAsync(
            eventItem,
            ticketTier,
            request,
            cancellationToken);
    }

    public async Task<PartnerBookingListResult> GetBookingsAsync(
        int partnerEventId,
        string organizerName,
        CancellationToken cancellationToken = default)
    {
        var eventItem = await _dbContext.PartnerEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == partnerEventId,
                cancellationToken);

        if (eventItem is null)
        {
            return PartnerBookingListResult.Failure(
                StatusCodes.Status404NotFound,
                "The connected event was not found.");
        }

        if (!NamesMatch(eventItem.OrganizerName, organizerName))
        {
            return PartnerBookingListResult.Failure(
                StatusCodes.Status403Forbidden,
                "Only the connected event organizer can view these ticket sales.");
        }

        try
        {
            var bookings = await _bookingClient.GetBookingsAsync(
                eventItem.SourceEventId,
                cancellationToken);

            return PartnerBookingListResult.Success(bookings);
        }
        catch (HttpRequestException)
        {
            return PartnerBookingListResult.Failure(
                StatusCodes.Status503ServiceUnavailable,
                "The connected organizer booking service is currently unavailable.");
        }
        catch (TaskCanceledException)
        {
            return PartnerBookingListResult.Failure(
                StatusCodes.Status504GatewayTimeout,
                "The connected organizer booking request took too long to complete.");
        }
    }

    public Task<PartnerBookingResponseDto?> GetBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default) =>
        _bookingClient.GetBookingAsync(bookingReference, cancellationToken);

    public Task<PartnerBookingOperationResult> CancelBookingAsync(
        string bookingReference,
        CancellationToken cancellationToken = default) =>
        _bookingClient.CancelBookingAsync(bookingReference, cancellationToken);
    private static bool NamesMatch(string left, string right) =>
        NormalizeName(left) == NormalizeName(right);

    private static string NormalizeName(string? value) =>
        string.Join(' ', (value ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Trim()
            .ToUpperInvariant();

}
