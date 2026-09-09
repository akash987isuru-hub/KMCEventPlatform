using System.Security.Claims;
using KMC.Api.DTOs.Events;
using KMC.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
    [FromQuery] EventSearchRequestDto search,
    CancellationToken cancellationToken)
    {
        var events = await _eventService.GetAllAsync(
            search,
            cancellationToken);

        return Ok(events);
    }

    [HttpGet("{eventId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        int eventId,
        CancellationToken cancellationToken)
    {
        var eventItem = await _eventService.GetByIdAsync(
            eventId,
            cancellationToken);

        if (eventItem is null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        return Ok(eventItem);
    }

    [HttpGet("my-events")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetMyEvents(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var organizerId))
        {
            return Unauthorized();
        }

        var events =
            await _eventService.GetOrganizerEventsAsync(
                organizerId,
                cancellationToken);

        return Ok(events);
    }

    [HttpPost]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Create(
        CreateEventRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var organizerId))
        {
            return Unauthorized();
        }

        var result = await _eventService.CreateAsync(
            request,
            organizerId,
            cancellationToken);

        if (result.Status == EventOperationStatus.Invalid)
        {
            return BadRequest(new
            {
                message = result.Message
            });
        }

        if (result.Status == EventOperationStatus.Forbidden)
        {
            return Forbid();
        }

        return CreatedAtAction(
            nameof(GetById),
            new { eventId = result.Data!.Id },
            result.Data);
    }

    [HttpPut("{eventId:int}")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Update(
        int eventId,
        UpdateEventRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var organizerId))
        {
            return Unauthorized();
        }

        var result = await _eventService.UpdateAsync(
            eventId,
            request,
            organizerId,
            cancellationToken);

        return result.Status switch
        {
            EventOperationStatus.Success =>
                Ok(result.Data),

            EventOperationStatus.Invalid =>
                BadRequest(new { message = result.Message }),

            EventOperationStatus.NotFound =>
                NotFound(new { message = result.Message }),

            EventOperationStatus.Forbidden =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = result.Message }),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("{eventId:int}")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Delete(
        int eventId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var organizerId))
        {
            return Unauthorized();
        }

        var result = await _eventService.DeleteAsync(
            eventId,
            organizerId,
            cancellationToken);

        return result.Status switch
        {
            EventOperationStatus.Success =>
                Ok(new { message = result.Message }),

            EventOperationStatus.Invalid =>
                BadRequest(new { message = result.Message }),

            EventOperationStatus.NotFound =>
                NotFound(new { message = result.Message }),

            EventOperationStatus.Forbidden =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    new { message = result.Message }),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError)
        };
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userIdValue, out userId);
    }
}