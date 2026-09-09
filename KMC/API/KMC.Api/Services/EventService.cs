using AutoMapper;
using KMC.Api.Data;
using KMC.Api.DTOs.Events;
using KMC.Api.Entities;
using KMC.Api.Infrastructure;
using KMC.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KMC.Api.Services;

public class EventService : IEventService
{
    private const int RequiredTicketTierCount = 3;

    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public EventService(
        ApplicationDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EventOperationResult> CreateAsync(
        CreateEventRequestDto request,
        int organizerId,
        CancellationToken cancellationToken = default)
    {
        var validationMessage = ValidateEventRequest(
            request.Title,
            request.EventType,
            request.Venue,
            request.Location,
            request.EventDate,
            request.EndDate,
            request.StartTime,
            request.EndTime,
            request.TicketTiers,
            requireFutureStart: true);

        if (validationMessage is not null)
        {
            return Invalid(validationMessage);
        }

        if (request.Status is not
            (EventStatus.Draft or EventStatus.Published))
        {
            return Invalid(
                "A new event must be created as Draft or Published.");
        }

        var organizerExists = await _context.Users.AnyAsync(
            user =>
                user.Id == organizerId &&
                user.Role == UserRole.Organizer,
            cancellationToken);

        if (!organizerExists)
        {
            return new EventOperationResult
            {
                Status = EventOperationStatus.Forbidden,
                Message = "A valid organizer account is required."
            };
        }

        var eventEntity = _mapper.Map<Event>(request);

        ApplyNormalizedValues(
            eventEntity,
            request.Title,
            request.Description,
            request.EventType,
            request.Venue,
            request.Location);

        eventEntity.EventDate = request.EventDate.Date;
        eventEntity.EndDate = request.EndDate.Date;
        eventEntity.OrganizerId = organizerId;
        eventEntity.CreatedAt = DateTime.UtcNow;
        eventEntity.Capacity = request.TicketTiers.Sum(
            ticketTier => ticketTier.Capacity);

        eventEntity.TicketTiers = request.TicketTiers
            .OrderBy(ticketTier => ticketTier.SortOrder)
            .Select(ticketTier => new TicketTier
            {
                Name = ticketTier.Name.Trim(),
                Price = decimal.Round(
                    ticketTier.Price,
                    2,
                    MidpointRounding.AwayFromZero),
                Capacity = ticketTier.Capacity,
                SortOrder = ticketTier.SortOrder
            })
            .ToList();

        _context.Events.Add(eventEntity);
        await _context.SaveChangesAsync(cancellationToken);

        var createdEvent = await GetEventEntityAsync(
            eventEntity.Id,
            cancellationToken);

        return new EventOperationResult
        {
            Status = EventOperationStatus.Success,
            Message = "Event and ticket categories created successfully.",
            Data = createdEvent is null
                ? null
                : _mapper.Map<EventResponseDto>(createdEvent)
        };
    }

    public async Task<IReadOnlyList<EventResponseDto>> GetAllAsync(
        EventSearchRequestDto search,
        CancellationToken cancellationToken = default)
    {
        var now = KmcTime.Now;
        var today = now.Date;
        var currentTime = now.TimeOfDay;

        // Published events remain discoverable after completion so the UI can
        // clearly display the Ended state instead of silently removing them.
        var query = BuildEventQuery()
            .Where(eventItem =>
                eventItem.Status == EventStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search.Search))
        {
            var searchPattern = $"%{search.Search.Trim()}%";

            query = query.Where(eventItem =>
                EF.Functions.Like(
                    eventItem.Title,
                    searchPattern) ||
                (eventItem.Description != null &&
                 EF.Functions.Like(
                     eventItem.Description,
                     searchPattern)) ||
                EF.Functions.Like(
                    eventItem.Organizer.FullName,
                    searchPattern));
        }

        if (!string.IsNullOrWhiteSpace(search.EventType))
        {
            var eventTypePattern =
                $"%{search.EventType.Trim()}%";

            query = query.Where(eventItem =>
                EF.Functions.Like(
                    eventItem.EventType,
                    eventTypePattern));
        }

        if (search.Date.HasValue)
        {
            var selectedDate = search.Date.Value.Date;

            query = query.Where(eventItem =>
                eventItem.EventDate <= selectedDate &&
                eventItem.EndDate >= selectedDate);
        }

        if (!string.IsNullOrWhiteSpace(search.Venue))
        {
            var venuePattern = $"%{search.Venue.Trim()}%";

            query = query.Where(eventItem =>
                EF.Functions.Like(
                    eventItem.Venue,
                    venuePattern));
        }

        if (!string.IsNullOrWhiteSpace(search.Location))
        {
            var locationPattern =
                $"%{search.Location.Trim()}%";

            query = query.Where(eventItem =>
                EF.Functions.Like(
                    eventItem.Location,
                    locationPattern));
        }

        var events = await query
            .OrderBy(eventItem =>
                eventItem.EndDate < today ||
                (eventItem.EndDate == today &&
                 eventItem.EndTime <= currentTime))
            .ThenBy(eventItem => eventItem.EventDate)
            .ThenBy(eventItem => eventItem.StartTime)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<EventResponseDto>>(events);
    }

    public async Task<EventResponseDto?> GetByIdAsync(
        int eventId,
        CancellationToken cancellationToken = default)
    {
        var eventEntity = await BuildEventQuery()
            .FirstOrDefaultAsync(
                eventItem =>
                    eventItem.Id == eventId &&
                    eventItem.Status == EventStatus.Published,
                cancellationToken);

        return eventEntity is null
            ? null
            : _mapper.Map<EventResponseDto>(eventEntity);
    }

    public async Task<IReadOnlyList<EventResponseDto>>
        GetOrganizerEventsAsync(
            int organizerId,
            CancellationToken cancellationToken = default)
    {
        var events = await BuildEventQuery()
            .Where(eventItem =>
                eventItem.OrganizerId == organizerId)
            .OrderByDescending(eventItem =>
                eventItem.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<EventResponseDto>>(events);
    }

    public async Task<EventOperationResult> UpdateAsync(
        int eventId,
        UpdateEventRequestDto request,
        int organizerId,
        CancellationToken cancellationToken = default)
    {
        var validationMessage = ValidateEventRequest(
            request.Title,
            request.EventType,
            request.Venue,
            request.Location,
            request.EventDate,
            request.EndDate,
            request.StartTime,
            request.EndTime,
            request.TicketTiers,
            requireFutureStart:
                request.Status is EventStatus.Draft or EventStatus.Published);

        if (validationMessage is not null)
        {
            return Invalid(validationMessage);
        }

        var eventEntity = await _context.Events
            .Include(eventItem => eventItem.Registrations)
            .Include(eventItem => eventItem.TicketTiers)
                .ThenInclude(ticketTier => ticketTier.Registrations)
            .FirstOrDefaultAsync(
                eventItem => eventItem.Id == eventId,
                cancellationToken);

        if (eventEntity is null)
        {
            return new EventOperationResult
            {
                Status = EventOperationStatus.NotFound,
                Message = "Event not found."
            };
        }

        if (eventEntity.OrganizerId != organizerId)
        {
            return new EventOperationResult
            {
                Status = EventOperationStatus.Forbidden,
                Message =
                    "Only the event creator can update this event."
            };
        }

        var invalidTierId = request.TicketTiers.Any(
            ticketTier =>
                ticketTier.Id > 0 &&
                eventEntity.TicketTiers.All(
                    existing => existing.Id != ticketTier.Id));

        if (invalidTierId)
        {
            return Invalid(
                "One or more ticket categories do not belong to this event.");
        }

        foreach (var existingTier in eventEntity.TicketTiers.ToList())
        {
            var matchingRequest = request.TicketTiers.FirstOrDefault(
                ticketTier => ticketTier.Id == existingTier.Id);

            var confirmedForTier = existingTier.Registrations
                .Where(registration =>
                    registration.Status == RegistrationStatus.Confirmed)
                .Sum(registration => registration.Quantity);

            if (matchingRequest is null)
            {
                if (confirmedForTier > 0)
                {
                    return Invalid(
                        $"The {existingTier.Name} category cannot be removed because it has confirmed ticket sales.");
                }

                _context.TicketTiers.Remove(existingTier);
                continue;
            }

            if (matchingRequest.Capacity < confirmedForTier)
            {
                return Invalid(
                    $"The {existingTier.Name} capacity cannot be lower than its confirmed ticket count ({confirmedForTier}).");
            }

            existingTier.Name = matchingRequest.Name.Trim();
            existingTier.Price = decimal.Round(
                matchingRequest.Price,
                2,
                MidpointRounding.AwayFromZero);
            existingTier.Capacity = matchingRequest.Capacity;
            existingTier.SortOrder = matchingRequest.SortOrder;
        }

        foreach (var newTier in request.TicketTiers.Where(
                     ticketTier => ticketTier.Id == 0))
        {
            eventEntity.TicketTiers.Add(new TicketTier
            {
                Name = newTier.Name.Trim(),
                Price = decimal.Round(
                    newTier.Price,
                    2,
                    MidpointRounding.AwayFromZero),
                Capacity = newTier.Capacity,
                SortOrder = newTier.SortOrder
            });
        }

        _mapper.Map(request, eventEntity);

        ApplyNormalizedValues(
            eventEntity,
            request.Title,
            request.Description,
            request.EventType,
            request.Venue,
            request.Location);

        eventEntity.EventDate = request.EventDate.Date;
        eventEntity.EndDate = request.EndDate.Date;
        eventEntity.Capacity = request.TicketTiers.Sum(
            ticketTier => ticketTier.Capacity);
        eventEntity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var updatedEvent = await GetEventEntityAsync(
            eventId,
            cancellationToken);

        return new EventOperationResult
        {
            Status = EventOperationStatus.Success,
            Message = "Event and ticket categories updated successfully.",
            Data = updatedEvent is null
                ? null
                : _mapper.Map<EventResponseDto>(updatedEvent)
        };
    }

    public async Task<EventOperationResult> DeleteAsync(
        int eventId,
        int organizerId,
        CancellationToken cancellationToken = default)
    {
        var eventEntity = await _context.Events
            .Include(eventItem => eventItem.Registrations)
                .ThenInclude(registration => registration.Payment)
            .Include(eventItem => eventItem.TicketTiers)
            .FirstOrDefaultAsync(
                eventItem => eventItem.Id == eventId,
                cancellationToken);

        if (eventEntity is null)
        {
            return new EventOperationResult
            {
                Status = EventOperationStatus.NotFound,
                Message = "Event not found."
            };
        }

        if (eventEntity.OrganizerId != organizerId)
        {
            return new EventOperationResult
            {
                Status = EventOperationStatus.Forbidden,
                Message =
                    "Only the event creator can delete this event."
            };
        }

        // The creator may delete the event at any time. Dependent demo
        // payment and ticket records are removed in a controlled order.
        _context.Payments.RemoveRange(
            eventEntity.Registrations
                .Where(registration => registration.Payment is not null)
                .Select(registration => registration.Payment!));

        _context.Registrations.RemoveRange(eventEntity.Registrations);
        _context.TicketTiers.RemoveRange(eventEntity.TicketTiers);
        _context.Events.Remove(eventEntity);

        await _context.SaveChangesAsync(cancellationToken);

        return new EventOperationResult
        {
            Status = EventOperationStatus.Success,
            Message =
                "Event and its related ticket records were deleted successfully."
        };
    }

    private IQueryable<Event> BuildEventQuery()
    {
        return _context.Events
            .AsNoTracking()
            .Include(eventItem => eventItem.Organizer)
            .Include(eventItem => eventItem.Registrations)
            .Include(eventItem => eventItem.TicketTiers)
                .ThenInclude(ticketTier => ticketTier.Registrations);
    }

    private async Task<Event?> GetEventEntityAsync(
        int eventId,
        CancellationToken cancellationToken)
    {
        return await BuildEventQuery()
            .FirstOrDefaultAsync(
                eventItem => eventItem.Id == eventId,
                cancellationToken);
    }

    private static string? ValidateEventRequest(
        string title,
        string eventType,
        string venue,
        string location,
        DateTime eventDate,
        DateTime endDate,
        TimeSpan startTime,
        TimeSpan endTime,
        IReadOnlyCollection<TicketTierRequestDto> ticketTiers,
        bool requireFutureStart)
    {
        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(eventType) ||
            string.IsNullOrWhiteSpace(venue) ||
            string.IsNullOrWhiteSpace(location))
        {
            return "Title, event type, venue and location are required.";
        }

        var eventStartDateTime =
            eventDate.Date.Add(startTime);
        var eventEndDateTime =
            endDate.Date.Add(endTime);

        if (requireFutureStart &&
            eventStartDateTime <= KmcTime.Now)
        {
            return "The event start date and time must be in the future for Draft or Published events.";
        }

        if (eventEndDateTime <= eventStartDateTime)
        {
            return "The event end date and time must be later than the start date and time.";
        }

        if (ticketTiers.Count != RequiredTicketTierCount)
        {
            return "Exactly three ticket categories are required (for example Standard, Premium and VIP).";
        }

        if (ticketTiers.Any(ticketTier =>
                string.IsNullOrWhiteSpace(ticketTier.Name) ||
                ticketTier.Capacity <= 0 ||
                ticketTier.Price < 0))
        {
            return "Each ticket category requires a name, a non-negative price and a capacity greater than zero.";
        }

        var uniqueNames = ticketTiers
            .Select(ticketTier => ticketTier.Name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        if (uniqueNames != RequiredTicketTierCount)
        {
            return "Ticket category names must be unique.";
        }

        var uniqueSortOrders = ticketTiers
            .Select(ticketTier => ticketTier.SortOrder)
            .Distinct()
            .Count();

        if (uniqueSortOrders != RequiredTicketTierCount ||
            ticketTiers.Any(ticketTier =>
                ticketTier.SortOrder is < 1 or > 3))
        {
            return "Ticket categories must use sort orders 1, 2 and 3.";
        }

        if (ticketTiers.Sum(ticketTier => ticketTier.Capacity) > 100000)
        {
            return "The total ticket capacity cannot exceed 100,000.";
        }

        return null;
    }

    private static void ApplyNormalizedValues(
        Event eventEntity,
        string title,
        string? description,
        string eventType,
        string venue,
        string location)
    {
        eventEntity.Title = title.Trim();
        eventEntity.EventType = eventType.Trim();
        eventEntity.Venue = venue.Trim();
        eventEntity.Location = location.Trim();
        eventEntity.Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    private static EventOperationResult Invalid(string message)
    {
        return new EventOperationResult
        {
            Status = EventOperationStatus.Invalid,
            Message = message
        };
    }
}
