using Microsoft.AspNetCore.Mvc;
using Organizer.Api.DTOs;
using Organizer.Api.Interfaces;

namespace Organizer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IOrganizerBookingService _bookingService;

    public BookingsController(IOrganizerBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("events/{externalEventCode}")]
    public async Task<IActionResult> Purchase(
        string externalEventCode,
        PurchaseOrganizerTicketRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var booking = await _bookingService.PurchaseAsync(
                externalEventCode,
                request,
                cancellationToken);

            return Ok(booking);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{bookingReference}")]
    public async Task<IActionResult> GetByReference(
        string bookingReference,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingService.GetByReferenceAsync(
            bookingReference,
            cancellationToken);

        return booking is null
            ? NotFound(new { message = "Booking was not found." })
            : Ok(booking);
    }

    [HttpDelete("{bookingReference}")]
    public async Task<IActionResult> Cancel(
        string bookingReference,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingService.CancelAsync(
            bookingReference,
            cancellationToken);

        return booking is null
            ? NotFound(new { message = "Booking was not found." })
            : Ok(booking);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? eventId,
        CancellationToken cancellationToken)
    {
        var bookings = await _bookingService.GetAllAsync(
            eventId,
            cancellationToken);

        return Ok(bookings);
    }
}
