using KMC.Api.Data;
using KMC.Api.DTOs.PartnerEvents;
using KMC.Api.Entities;
using KMC.Api.Infrastructure;
using KMC.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KMC.Api.Services;

public class PartnerEventService : IPartnerEventService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IOrganizerPartnerEventClient _organizerEventClient;

    public PartnerEventService(
        ApplicationDbContext dbContext,
        IOrganizerPartnerEventClient organizerEventClient)
    {
        _dbContext = dbContext;
        _organizerEventClient = organizerEventClient;
    }

    public async Task<IReadOnlyList<PartnerEventResponseDto>> GetAllAsync(
        PartnerEventSearchRequestDto search,
        CancellationToken cancellationToken = default)
    {
        var now = KmcTime.Now;
        var today = now.Date;
        var currentTime = now.TimeOfDay;

        var query = _dbContext.PartnerEvents
            .Include(item => item.TicketTiers)
            .AsNoTracking()
            .Where(item => item.IsPublished);

        if (!string.IsNullOrWhiteSpace(search.Search))
        {
            var value = search.Search.Trim();
            query = query.Where(item =>
                item.Title.Contains(value) ||
                (item.Description != null && item.Description.Contains(value)) ||
                item.OrganizerName.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(search.EventType))
        {
            var value = search.EventType.Trim();
            query = query.Where(item => item.EventType.Contains(value));
        }

        if (search.Date.HasValue)
        {
            var date = search.Date.Value.Date;
            query = query.Where(item =>
                item.EventDate <= date &&
                item.EndDate >= date);
        }

        if (!string.IsNullOrWhiteSpace(search.Venue))
        {
            var value = search.Venue.Trim();
            query = query.Where(item => item.Venue.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(search.Location))
        {
            var value = search.Location.Trim();
            query = query.Where(item => item.Location.Contains(value));
        }

        var events = await query
            .OrderBy(item =>
                item.EndDate < today ||
                (item.EndDate == today &&
                 item.EndTime <= currentTime))
            .ThenBy(item => item.EventDate)
            .ThenBy(item => item.StartTime)
            .ToListAsync(cancellationToken);

        return events.Select(MapToResponse).ToList();
    }

    public async Task<PartnerEventResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var eventItem = await _dbContext.PartnerEvents
            .Include(item => item.TicketTiers)
            .AsNoTracking()
            .FirstOrDefaultAsync(
                item => item.Id == id && item.IsPublished,
                cancellationToken);

        return eventItem is null ? null : MapToResponse(eventItem);
    }

    public async Task<IReadOnlyList<PartnerEventResponseDto>> GetManagedAsync(
        string organizerName,
        CancellationToken cancellationToken = default)
    {
        var events = await _dbContext.PartnerEvents
            .Include(item => item.TicketTiers)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return events
            .Where(item => NamesMatch(item.OrganizerName, organizerName))
            .OrderBy(item => item.EndDate.Date.Add(item.EndTime) <= KmcTime.Now)
            .ThenBy(item => item.EventDate)
            .ThenBy(item => item.StartTime)
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<PartnerEventResponseDto?> GetManagedByIdAsync(
        int id,
        string organizerName,
        CancellationToken cancellationToken = default)
    {
        var eventItem = await _dbContext.PartnerEvents
            .Include(item => item.TicketTiers)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return eventItem is null || !NamesMatch(eventItem.OrganizerName, organizerName)
            ? null
            : MapToResponse(eventItem);
    }

    public async Task<PartnerEventResponseDto> SyncAsync(
        PartnerEventSyncRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateEvent(
            request.EventDate,
            request.EndDate,
            request.StartTime,
            request.EndTime);
        ValidateTicketTiers(request.TicketTiers);

        var externalEventCode = request.ExternalEventCode.Trim();
        var eventItem = await _dbContext.PartnerEvents
            .Include(item => item.TicketTiers)
            .FirstOrDefaultAsync(
                item => item.ExternalEventCode == externalEventCode,
                cancellationToken);

        var now = DateTime.UtcNow;
        if (eventItem is null)
        {
            eventItem = new PartnerEvent
            {
                ExternalEventCode = externalEventCode,
                CreatedAt = now
            };
            _dbContext.PartnerEvents.Add(eventItem);
        }
        else
        {
            eventItem.UpdatedAt = now;
        }

        eventItem.SourceEventId = request.SourceEventId;
        eventItem.SourceSystem = request.SourceSystem.Trim();
        eventItem.Title = request.Title.Trim();
        eventItem.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();
        eventItem.EventType = request.EventType.Trim();
        eventItem.EventDate = request.EventDate.Date;
        eventItem.EndDate = request.EndDate.Date;
        eventItem.StartTime = request.StartTime;
        eventItem.EndTime = request.EndTime;
        eventItem.Venue = request.Venue.Trim();
        eventItem.Location = request.Location.Trim();
        eventItem.OrganizerName = request.OrganizerName.Trim();
        eventItem.OrganizerEventUrl = request.OrganizerEventUrl.Trim();
        eventItem.IsPublished = request.IsPublished;
        eventItem.LastSyncedAt = now;

        UpdateTicketTiers(eventItem, request.TicketTiers);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(eventItem);
    }

    public async Task<PartnerEventManagementResult> UpdateManagedAsync(
        int id,
        PartnerEventUpdateRequestDto request,
        string organizerName,
        CancellationToken cancellationToken = default)
    {
        var eventItem = await _dbContext.PartnerEvents
            .Include(item => item.TicketTiers)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (eventItem is null)
        {
            return PartnerEventManagementResult.Failure(
                StatusCodes.Status404NotFound,
                "The connected event was not found.");
        }

        if (!NamesMatch(eventItem.OrganizerName, organizerName))
        {
            return PartnerEventManagementResult.Failure(
                StatusCodes.Status403Forbidden,
                "Only the connected event organizer can update this event.");
        }

        var invalidTier = request.TicketTiers.Any(requestTier =>
            requestTier.ExternalTicketTierId > 0 &&
            eventItem.TicketTiers.All(existing =>
                existing.ExternalTicketTierId != requestTier.ExternalTicketTierId));

        if (invalidTier)
        {
            return PartnerEventManagementResult.Failure(
                StatusCodes.Status400BadRequest,
                "One or more ticket categories do not belong to this connected event.");
        }

        var updateResult = await _organizerEventClient.UpdateAsync(
            eventItem,
            request,
            cancellationToken);

        if (!updateResult.IsSuccess)
        {
            return updateResult;
        }

        var refreshed = await _dbContext.PartnerEvents
            .Include(item => item.TicketTiers)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return refreshed is null
            ? PartnerEventManagementResult.Failure(
                StatusCodes.Status502BadGateway,
                "The connected event was updated, but the refreshed KMC listing could not be loaded.")
            : PartnerEventManagementResult.Success(
                MapToResponse(refreshed),
                updateResult.Message);
    }

    public async Task<PartnerEventManagementResult> DeleteManagedAsync(
        int id,
        string organizerName,
        CancellationToken cancellationToken = default)
    {
        var eventItem = await _dbContext.PartnerEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (eventItem is null)
        {
            return PartnerEventManagementResult.Failure(
                StatusCodes.Status404NotFound,
                "The connected event was not found.");
        }

        if (!NamesMatch(eventItem.OrganizerName, organizerName))
        {
            return PartnerEventManagementResult.Failure(
                StatusCodes.Status403Forbidden,
                "Only the connected event organizer can delete this event.");
        }

        var deleteResult = await _organizerEventClient.DeleteAsync(
            eventItem,
            cancellationToken);

        if (!deleteResult.IsSuccess)
        {
            return deleteResult;
        }

        // The organizer API normally removes the synchronized KMC record via
        // the partner callback. Remove any remaining local copy as a safe fallback.
        var localCopy = await _dbContext.PartnerEvents
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (localCopy is not null)
        {
            _dbContext.PartnerEvents.Remove(localCopy);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return deleteResult;
    }

    public async Task<bool> DeleteByExternalCodeAsync(
        string externalEventCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCode = externalEventCode.Trim();
        var eventItem = await _dbContext.PartnerEvents
            .FirstOrDefaultAsync(
                item => item.ExternalEventCode == normalizedCode,
                cancellationToken);

        if (eventItem is null)
        {
            return false;
        }

        _dbContext.PartnerEvents.Remove(eventItem);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void UpdateTicketTiers(
        PartnerEvent eventItem,
        IReadOnlyList<PartnerEventTicketTierSyncDto> requests)
    {
        var externalIds = requests.Select(item => item.ExternalTicketTierId).ToHashSet();

        foreach (var existing in eventItem.TicketTiers.ToList())
        {
            if (!externalIds.Contains(existing.ExternalTicketTierId))
            {
                _dbContext.PartnerEventTicketTiers.Remove(existing);
            }
        }

        foreach (var request in requests)
        {
            var tier = eventItem.TicketTiers.FirstOrDefault(
                item => item.ExternalTicketTierId == request.ExternalTicketTierId);

            if (tier is null)
            {
                tier = new PartnerEventTicketTier
                {
                    ExternalTicketTierId = request.ExternalTicketTierId
                };
                eventItem.TicketTiers.Add(tier);
            }

            tier.Name = request.Name.Trim();
            tier.Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();
            tier.Price = request.Price;
            tier.Capacity = request.Capacity;
            tier.SoldCount = Math.Min(request.SoldCount, request.Capacity);
            tier.SortOrder = request.SortOrder;
        }
    }

    private static void ValidateEvent(
        DateTime eventDate,
        DateTime endDate,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        var start = eventDate.Date.Add(startTime);
        var end = endDate.Date.Add(endTime);

        if (end <= start)
        {
            throw new ArgumentException(
                "Event end date and time must be later than the start date and time.");
        }
    }

    private static void ValidateTicketTiers(
        IReadOnlyList<PartnerEventTicketTierSyncDto> tiers)
    {
        if (tiers.Count is < 1 or > 5)
        {
            throw new ArgumentException("Partner event must contain between one and five ticket categories.");
        }

        if (tiers.Any(item => item.SoldCount > item.Capacity))
        {
            throw new ArgumentException("Ticket sold count cannot be higher than capacity.");
        }
    }

    private static bool NamesMatch(string left, string right) =>
        NormalizeName(left) == NormalizeName(right);

    private static string NormalizeName(string? value) =>
        string.Join(' ', (value ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Trim()
            .ToUpperInvariant();

    private static PartnerEventResponseDto MapToResponse(PartnerEvent eventItem)
    {
        var tiers = eventItem.TicketTiers
            .OrderBy(item => item.SortOrder)
            .Select(item => new PartnerEventTicketTierResponseDto
            {
                Id = item.Id,
                ExternalTicketTierId = item.ExternalTicketTierId,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Capacity = item.Capacity,
                SoldCount = item.SoldCount,
                SortOrder = item.SortOrder
            })
            .ToList();

        return new PartnerEventResponseDto
        {
            Id = eventItem.Id,
            ExternalEventCode = eventItem.ExternalEventCode,
            SourceEventId = eventItem.SourceEventId,
            SourceSystem = eventItem.SourceSystem,
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
            OrganizerEventUrl = eventItem.OrganizerEventUrl,
            IsPublished = eventItem.IsPublished,
            TotalCapacity = tiers.Sum(item => item.Capacity),
            RegisteredParticipantCount = tiers.Sum(item => item.SoldCount),
            MinimumTicketPrice = tiers.Count == 0 ? 0 : tiers.Min(item => item.Price),
            TicketTiers = tiers,
            CreatedAt = eventItem.CreatedAt,
            UpdatedAt = eventItem.UpdatedAt,
            LastSyncedAt = eventItem.LastSyncedAt
        };
    }
}
