using Microsoft.AspNetCore.Mvc;
using Organizer.Api.DTOs;
using Organizer.Api.Interfaces;

namespace Organizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IOrganizerEventService _eventService;

    public EventsController(
        IOrganizerEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<
        IReadOnlyList<OrganizerEventResponseDto>>>
        GetAllAsync(
            CancellationToken cancellationToken)
    {
        var events = await _eventService.GetAllAsync(
            cancellationToken);

        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<
        OrganizerEventResponseDto>>
        GetByIdAsync(
            int id,
            CancellationToken cancellationToken)
    {
        var eventItem =
            await _eventService.GetByIdAsync(
                id,
                cancellationToken);

        if (eventItem is null)
        {
            return NotFound(new
            {
                message = "Event was not found."
            });
        }

        return Ok(eventItem);
    }

    [HttpPost]
    public async Task<ActionResult<
        OrganizerEventResponseDto>>
        CreateAsync(
            CreateOrganizerEventRequestDto request,
            CancellationToken cancellationToken)
    {
        try
        {
            var createdEvent =
                await _eventService.CreateAsync(
                    request,
                    cancellationToken);

            return CreatedAtAction(
                nameof(GetByIdAsync),
                new
                {
                    id = createdEvent.Id
                },
                createdEvent);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<
        OrganizerEventResponseDto>>
        UpdateAsync(
            int id,
            UpdateOrganizerEventRequestDto request,
            CancellationToken cancellationToken)
    {
        try
        {
            var updatedEvent =
                await _eventService.UpdateAsync(
                    id,
                    request,
                    cancellationToken);

            if (updatedEvent is null)
            {
                return NotFound(new
                {
                    message = "Event was not found."
                });
            }

            return Ok(updatedEvent);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPost("{id:int}/publish-to-kmc")]
    public async Task<ActionResult<OrganizerEventResponseDto>>
        PublishToKmcAsync(
            int id,
            CancellationToken cancellationToken)
    {
        var eventItem = await _eventService.PublishToKmcAsync(
            id,
            cancellationToken);

        if (eventItem is null)
        {
            return NotFound(new
            {
                message = "Event was not found."
            });
        }

        return eventItem.IsPublishedToKmc
            ? Ok(eventItem)
            : StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message = eventItem.KmcSyncError ??
                              "The event could not be published to KMC.",
                    eventItem
                });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult>
        DeleteAsync(
            int id,
            CancellationToken cancellationToken)
    {
        var deleted =
            await _eventService.DeleteAsync(
                id,
                cancellationToken);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Event was not found."
            });
        }

        return NoContent();
    }
}