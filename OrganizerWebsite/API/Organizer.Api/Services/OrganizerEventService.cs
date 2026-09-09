using Microsoft.EntityFrameworkCore;
using Organizer.Api.Data;
using Organizer.Api.DTOs;
using Organizer.Api.Entities;
using Organizer.Api.Interfaces;

namespace Organizer.Api.Services;

public class OrganizerEventService : IOrganizerEventService
{
    private readonly OrganizerDbContext _dbContext;
    private readonly IKmcPartnerEventClient _kmcPartnerEventClient;

    public OrganizerEventService(
        OrganizerDbContext dbContext,
        IKmcPartnerEventClient kmcPartnerEventClient)
    {
        _dbContext = dbContext;
        _kmcPartnerEventClient = kmcPartnerEventClient;
    }

    public async Task<IReadOnlyList<OrganizerEventResponseDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.Now;
        var events = await EventQuery()
            .AsNoTracking()
            .OrderBy(item =>
                item.EndDate < now.Date ||
                (item.EndDate == now.Date &&
                 item.EndTime <= now.TimeOfDay))
            .ThenBy(item => item.EventDate)
            .ThenBy(item => item.StartTime)
            .ToListAsync(cancellationToken);

        return events.Select(MapToResponse).ToList();
    }

    public async Task<OrganizerEventResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var eventItem = await EventQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return eventItem is null ? null : MapToResponse(eventItem);
    }

    public async Task<OrganizerEventResponseDto> CreateAsync(
        CreateOrganizerEventRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateEvent(
            request.EventDate,
            request.EndDate,
            request.StartTime,
            request.EndTime,
            requireFutureStart: true);
        ValidateTicketTiers(request.TicketTiers);

        var eventItem = new OrganizerEvent
        {
            ExternalEventCode = $"ORG-{Guid.NewGuid():N}",
            Title = request.Title.Trim(),
            Description = NormalizeOptional(request.Description),
            EventType = request.EventType.Trim(),
            EventDate = request.EventDate.Date,
            EndDate = request.EndDate.Date,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Venue = request.Venue.Trim(),
            Location = request.Location.Trim(),
            OrganizerName = request.OrganizerName.Trim(),
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow,
            TicketTiers = request.TicketTiers
                .OrderBy(item => item.SortOrder)
                .Select(MapNewTicketTier)
                .ToList()
        };

        _dbContext.Events.Add(eventItem);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await SynchronizeWithKmcAsync(eventItem, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(eventItem);
    }

    public async Task<OrganizerEventResponseDto?> UpdateAsync(
        int id,
        UpdateOrganizerEventRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateEvent(
            request.EventDate,
            request.EndDate,
            request.StartTime,
            request.EndTime,
            requireFutureStart: false);
        ValidateTicketTiers(request.TicketTiers);

        var eventItem = await EventQuery()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (eventItem is null)
        {
            return null;
        }

        eventItem.Title = request.Title.Trim();
        eventItem.Description = NormalizeOptional(request.Description);
        eventItem.EventType = request.EventType.Trim();
        eventItem.EventDate = request.EventDate.Date;
        eventItem.EndDate = request.EndDate.Date;
        eventItem.StartTime = request.StartTime;
        eventItem.EndTime = request.EndTime;
        eventItem.Venue = request.Venue.Trim();
        eventItem.Location = request.Location.Trim();
        eventItem.OrganizerName = request.OrganizerName.Trim();
        eventItem.IsPublished = request.IsPublished;
        eventItem.UpdatedAt = DateTime.UtcNow;

        UpdateTicketTiers(eventItem, request.TicketTiers);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await SynchronizeWithKmcAsync(eventItem, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(eventItem);
    }

    public async Task<OrganizerEventResponseDto?> PublishToKmcAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var eventItem = await EventQuery()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (eventItem is null)
        {
            return null;
        }

        await SynchronizeWithKmcAsync(eventItem, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(eventItem);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var eventItem = await _dbContext.Events
            .Include(item => item.TicketTiers)
            .Include(item => item.Bookings)
                .ThenInclude(item => item.Payment)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (eventItem is null)
        {
            return false;
        }

        await _kmcPartnerEventClient.DeleteAsync(
            eventItem.ExternalEventCode,
            cancellationToken);

        _dbContext.Payments.RemoveRange(
            eventItem.Bookings
                .Where(item => item.Payment is not null)
                .Select(item => item.Payment));

        _dbContext.Bookings.RemoveRange(eventItem.Bookings);
        _dbContext.TicketTiers.RemoveRange(eventItem.TicketTiers);
        _dbContext.Events.Remove(eventItem);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private IQueryable<OrganizerEvent> EventQuery() =>
        _dbContext.Events
            .Include(item => item.TicketTiers)
                .ThenInclude(item => item.Bookings);

    private async Task SynchronizeWithKmcAsync(
        OrganizerEvent eventItem,
        CancellationToken cancellationToken)
    {
        var result = await _kmcPartnerEventClient.SyncAsync(
            eventItem,
            cancellationToken);

        if (result.IsSuccess)
        {
            eventItem.IsPublishedToKmc = true;
            eventItem.KmcEventId = result.KmcEventId;
            eventItem.KmcSyncError = null;
            eventItem.PublishedToKmcAt = DateTime.UtcNow;
        }
        else
        {
            eventItem.IsPublishedToKmc = false;
            eventItem.KmcSyncError = result.ErrorMessage;
        }
    }

    private static OrganizerTicketTier MapNewTicketTier(
        OrganizerTicketTierRequestDto request) => new()
        {
            Name = request.Name.Trim(),
            Description = NormalizeOptional(request.Description),
            Price = decimal.Round(
                request.Price,
                2,
                MidpointRounding.AwayFromZero),
            Capacity = request.Capacity,
            SortOrder = request.SortOrder
        };

    private void UpdateTicketTiers(
        OrganizerEvent eventItem,
        IReadOnlyList<OrganizerTicketTierRequestDto> requests)
    {
        var requestedIds = requests
            .Where(item => item.Id > 0)
            .Select(item => item.Id)
            .ToHashSet();

        foreach (var existing in eventItem.TicketTiers.ToList())
        {
            if (requestedIds.Contains(existing.Id))
            {
                continue;
            }

            var soldCount = existing.Bookings
                .Where(item => item.Status == "Confirmed")
                .Sum(item => item.Quantity);

            if (soldCount > 0)
            {
                throw new ArgumentException(
                    $"The {existing.Name} ticket category cannot be removed because tickets have already been sold.");
            }

            _dbContext.TicketTiers.Remove(existing);
        }

        foreach (var request in requests.OrderBy(item => item.SortOrder))
        {
            var existing = request.Id > 0
                ? eventItem.TicketTiers.FirstOrDefault(item => item.Id == request.Id)
                : null;

            if (existing is null)
            {
                eventItem.TicketTiers.Add(MapNewTicketTier(request));
                continue;
            }

            var soldCount = existing.Bookings
                .Where(item => item.Status == "Confirmed")
                .Sum(item => item.Quantity);

            if (request.Capacity < soldCount)
            {
                throw new ArgumentException(
                    $"The {existing.Name} capacity cannot be lower than the {soldCount} ticket(s) already sold.");
            }

            existing.Name = request.Name.Trim();
            existing.Description = NormalizeOptional(request.Description);
            existing.Price = decimal.Round(
                request.Price,
                2,
                MidpointRounding.AwayFromZero);
            existing.Capacity = request.Capacity;
            existing.SortOrder = request.SortOrder;
        }
    }

    private static void ValidateEvent(
        DateTime eventDate,
        DateTime endDate,
        TimeSpan startTime,
        TimeSpan endTime,
        bool requireFutureStart)
    {
        var start = eventDate.Date.Add(startTime);
        var end = endDate.Date.Add(endTime);

        if (requireFutureStart && start <= DateTime.Now)
        {
            throw new ArgumentException(
                "Event start date and time must be in the future.");
        }

        if (end <= start)
        {
            throw new ArgumentException(
                "Event end date and time must be later than the start date and time.");
        }
    }

    private static void ValidateTicketTiers(
        IReadOnlyList<OrganizerTicketTierRequestDto> tiers)
    {
        if (tiers.Count is < 1 or > 5)
        {
            throw new ArgumentException("Add between one and five ticket categories.");
        }

        if (tiers.Any(item =>
                string.IsNullOrWhiteSpace(item.Name) ||
                item.Price < 0 ||
                item.Capacity <= 0))
        {
            throw new ArgumentException(
                "Each ticket category requires a name, non-negative price and capacity above zero.");
        }

        if (tiers.Select(item => item.Name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() != tiers.Count)
        {
            throw new ArgumentException("Ticket category names must be unique.");
        }

        if (tiers.Select(item => item.SortOrder).Distinct().Count() != tiers.Count)
        {
            throw new ArgumentException("Ticket category order values must be unique.");
        }
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static OrganizerEventResponseDto MapToResponse(
        OrganizerEvent eventItem)
    {
        var tiers = eventItem.TicketTiers
            .OrderBy(item => item.SortOrder)
            .Select(item =>
            {
                var soldCount = item.Bookings
                    .Where(booking => booking.Status == "Confirmed")
                    .Sum(booking => booking.Quantity);

                return new OrganizerTicketTierResponseDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    Capacity = item.Capacity,
                    SoldCount = soldCount,
                    SortOrder = item.SortOrder
                };
            })
            .ToList();

        return new OrganizerEventResponseDto
        {
            Id = eventItem.Id,
            ExternalEventCode = eventItem.ExternalEventCode,
            Title = eventItem.Title,
            Description = eventItem.Description,
            EventType = eventItem.EventType,
            EventDate = eventItem.EventDate,
            EndDate = eventItem.EndDate,
            StartTime = eventItem.StartTime,
            EndTime = eventItem.EndTime,
            Venue = eventItem.Venue,
            Location = eventItem.Location,
            OrganizerName = eventItem.OrganizerName,
            IsPublished = eventItem.IsPublished,
            IsPublishedToKmc = eventItem.IsPublishedToKmc,
            KmcEventId = eventItem.KmcEventId,
            KmcSyncError = eventItem.KmcSyncError,
            TotalCapacity = tiers.Sum(item => item.Capacity),
            SoldCount = tiers.Sum(item => item.SoldCount),
            MinimumTicketPrice = tiers.Count == 0 ? 0 : tiers.Min(item => item.Price),
            TicketTiers = tiers,
            CreatedAt = eventItem.CreatedAt,
            UpdatedAt = eventItem.UpdatedAt,
            PublishedToKmcAt = eventItem.PublishedToKmcAt
        };
    }
}
