using System.Data;
using Microsoft.EntityFrameworkCore;
using Organizer.Api.Data;
using Organizer.Api.DTOs;
using Organizer.Api.Entities;
using Organizer.Api.Interfaces;

namespace Organizer.Api.Services;

public class OrganizerBookingService : IOrganizerBookingService
{
    private readonly OrganizerDbContext _dbContext;
    private readonly IOrganizerEventService _eventService;

    public OrganizerBookingService(
        OrganizerDbContext dbContext,
        IOrganizerEventService eventService)
    {
        _dbContext = dbContext;
        _eventService = eventService;
    }

    public async Task<OrganizerBookingResponseDto> PurchaseAsync(
        string externalEventCode,
        PurchaseOrganizerTicketRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateExpiry(request.ExpiryMonth, request.ExpiryYear);

        if (request.Quantity is < 1 or > 10)
        {
            throw new ArgumentException("Choose between 1 and 10 tickets.");
        }

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var eventItem = await _dbContext.Events
            .Include(item => item.TicketTiers)
                .ThenInclude(item => item.Bookings)
            .FirstOrDefaultAsync(
                item => item.ExternalEventCode == externalEventCode,
                cancellationToken);

        if (eventItem is null)
        {
            throw new KeyNotFoundException("Event was not found.");
        }

        if (!eventItem.IsPublished)
        {
            throw new InvalidOperationException("This event is not currently open for bookings.");
        }

        var eventEnd = eventItem.EndDate.Date.Add(eventItem.EndTime);
        if (eventEnd <= DateTime.Now)
        {
            throw new InvalidOperationException("This event has ended.");
        }

        var tier = eventItem.TicketTiers
            .FirstOrDefault(item => item.Id == request.TicketTierId);

        if (tier is null)
        {
            throw new KeyNotFoundException("Ticket category was not found.");
        }

        var normalizedCustomerEmail = request.CustomerEmail.Trim().ToLowerInvariant();
        var alreadyPurchasedCategory = tier.Bookings.Any(item =>
            string.Equals(item.Status, "Confirmed", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(item.CustomerEmail, normalizedCustomerEmail, StringComparison.OrdinalIgnoreCase));

        if (alreadyPurchasedCategory)
        {
            throw new InvalidOperationException(
                "You already purchased this ticket category for this event. Each public user can purchase a category only once.");
        }

        var soldCount = tier.Bookings
            .Where(item => item.Status == "Confirmed")
            .Sum(item => item.Quantity);

        var available = tier.Capacity - soldCount;
        if (request.Quantity > available)
        {
            throw new InvalidOperationException(
                available <= 0
                    ? "This ticket category is sold out."
                    : $"Only {available} ticket(s) are currently available.");
        }

        var normalizedCardNumber = new string(
            request.CardNumber.Where(char.IsDigit).ToArray());

        if (normalizedCardNumber.Length != 16)
        {
            throw new ArgumentException("Enter a valid 16-digit card number.");
        }

        var now = DateTime.UtcNow;
        var totalAmount = decimal.Round(
            tier.Price * request.Quantity,
            2,
            MidpointRounding.AwayFromZero);

        var booking = new OrganizerBooking
        {
            BookingReference = $"SEK-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            EventId = eventItem.Id,
            Event = eventItem,
            TicketTierId = tier.Id,
            TicketTier = tier,
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = normalizedCustomerEmail,
            CustomerPhone = request.CustomerPhone.Trim(),
            Quantity = request.Quantity,
            UnitPrice = tier.Price,
            TotalAmount = totalAmount,
            BookingSource = string.IsNullOrWhiteSpace(request.BookingSource)
                ? "Sparkling Events Kandy"
                : request.BookingSource.Trim(),
            Status = "Confirmed",
            CreatedAt = now,
            Payment = new OrganizerPayment
            {
                Amount = totalAmount,
                Currency = "LKR",
                CardHolderName = request.CardHolderName.Trim(),
                CardBrand = DetermineCardBrand(normalizedCardNumber),
                CardLastFour = normalizedCardNumber[^4..],
                TransactionReference = $"PAY-{now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
                Status = "Approved",
                PaidAt = now
            }
        };

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // Re-sync ticket stock so purchases from either website are immediately
        // reflected by the KMC partner event.
        await _eventService.PublishToKmcAsync(eventItem.Id, cancellationToken);

        return MapToResponse(booking);
    }

    public async Task<OrganizerBookingResponseDto?> GetByReferenceAsync(
        string bookingReference,
        CancellationToken cancellationToken = default)
    {
        var normalized = bookingReference.Trim().ToUpperInvariant();
        var booking = await BookingQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.BookingReference == normalized,
                cancellationToken);

        return booking is null ? null : MapToResponse(booking);
    }

    public async Task<IReadOnlyList<OrganizerBookingResponseDto>> GetAllAsync(
        int? eventId = null,
        CancellationToken cancellationToken = default)
    {
        var query = BookingQuery().AsNoTracking();

        if (eventId.HasValue)
        {
            query = query.Where(item => item.EventId == eventId.Value);
        }

        var bookings = await query
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        return bookings.Select(MapToResponse).ToList();
    }

    public async Task<OrganizerBookingResponseDto?> CancelAsync(
        string bookingReference,
        CancellationToken cancellationToken = default)
    {
        var normalized = bookingReference.Trim().ToUpperInvariant();
        var booking = await BookingQuery()
            .FirstOrDefaultAsync(
                item => item.BookingReference == normalized,
                cancellationToken);

        if (booking is null)
        {
            return null;
        }

        if (booking.Status == "Confirmed")
        {
            booking.Status = "Cancelled";
            booking.Payment.Status = "Refunded";
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _eventService.PublishToKmcAsync(booking.EventId, cancellationToken);
        }

        return MapToResponse(booking);
    }

    private IQueryable<OrganizerBooking> BookingQuery() =>
        _dbContext.Bookings
            .Include(item => item.Event)
            .Include(item => item.TicketTier)
            .Include(item => item.Payment);

    private static void ValidateExpiry(int month, int year)
    {
        DateTime expiry;
        try
        {
            expiry = new DateTime(year, month, 1)
                .AddMonths(1)
                .AddDays(-1);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new ArgumentException("Enter a valid payment card expiry date.");
        }

        if (expiry.Date < DateTime.Today)
        {
            throw new ArgumentException("The payment card has expired.");
        }
    }

    private static string DetermineCardBrand(string cardNumber) =>
        cardNumber.StartsWith('4')
            ? "Visa"
            : cardNumber.StartsWith('5')
                ? "Mastercard"
                : "Card";

    private static OrganizerBookingResponseDto MapToResponse(
        OrganizerBooking booking) => new()
    {
        Id = booking.Id,
        BookingReference = booking.BookingReference,
        EventId = booking.EventId,
        EventTitle = booking.Event.Title,
        EventDate = booking.Event.EventDate,
        EndDate = booking.Event.EndDate,
        StartTime = booking.Event.StartTime,
        EndTime = booking.Event.EndTime,
        Venue = booking.Event.Venue,
        Location = booking.Event.Location,
        TicketTierId = booking.TicketTierId,
        TicketTierName = booking.TicketTier.Name,
        CustomerName = booking.CustomerName,
        CustomerEmail = booking.CustomerEmail,
        CustomerPhone = booking.CustomerPhone,
        Quantity = booking.Quantity,
        UnitPrice = booking.UnitPrice,
        TotalAmount = booking.TotalAmount,
        Currency = booking.Payment.Currency,
        BookingSource = booking.BookingSource,
        BookingStatus = booking.Status,
        PaymentStatus = booking.Payment.Status,
        TransactionReference = booking.Payment.TransactionReference,
        CardDisplay = $"{booking.Payment.CardBrand} •••• {booking.Payment.CardLastFour}",
        PaidAt = booking.Payment.PaidAt,
        CreatedAt = booking.CreatedAt
    };
}
