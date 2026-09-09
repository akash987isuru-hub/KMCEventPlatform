using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KMC.Api.DTOs.PartnerEvents;
using KMC.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartnerEventsController : ControllerBase
{
    private const string ApiKeyHeaderName = "X-Partner-Api-Key";

    private readonly IPartnerEventService _partnerEventService;
    private readonly IPartnerBookingService _partnerBookingService;
    private readonly IConfiguration _configuration;

    public PartnerEventsController(
        IPartnerEventService partnerEventService,
        IPartnerBookingService partnerBookingService,
        IConfiguration configuration)
    {
        _partnerEventService = partnerEventService;
        _partnerBookingService = partnerBookingService;
        _configuration = configuration;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] PartnerEventSearchRequestDto search,
        CancellationToken cancellationToken)
    {
        return Ok(await _partnerEventService.GetAllAsync(search, cancellationToken));
    }

    [HttpGet("managed")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetManaged(
        CancellationToken cancellationToken)
    {
        var organizerName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(organizerName))
        {
            return Unauthorized(new { message = "The organizer login session is invalid." });
        }

        return Ok(await _partnerEventService.GetManagedAsync(
            organizerName,
            cancellationToken));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var eventItem = await _partnerEventService.GetByIdAsync(id, cancellationToken);
        return eventItem is null
            ? NotFound(new { message = "Partner event was not found." })
            : Ok(eventItem);
    }

    [HttpGet("{id:int}/manage")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetManagedById(
        int id,
        CancellationToken cancellationToken)
    {
        var organizerName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(organizerName))
        {
            return Unauthorized(new { message = "The organizer login session is invalid." });
        }

        var eventItem = await _partnerEventService.GetManagedByIdAsync(
            id,
            organizerName,
            cancellationToken);

        return eventItem is null
            ? NotFound(new { message = "The connected event was not found for this organizer." })
            : Ok(eventItem);
    }

    [HttpPut("{id:int}/manage")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> UpdateManaged(
        int id,
        PartnerEventUpdateRequestDto request,
        CancellationToken cancellationToken)
    {
        var organizerName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(organizerName))
        {
            return Unauthorized(new { message = "The organizer login session is invalid." });
        }

        var result = await _partnerEventService.UpdateManagedAsync(
            id,
            request,
            organizerName,
            cancellationToken);

        return result.IsSuccess && result.Event is not null
            ? Ok(result.Event)
            : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpDelete("{id:int}/manage")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> DeleteManaged(
        int id,
        CancellationToken cancellationToken)
    {
        var organizerName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(organizerName))
        {
            return Unauthorized(new { message = "The organizer login session is invalid." });
        }

        var result = await _partnerEventService.DeleteManagedAsync(
            id,
            organizerName,
            cancellationToken);

        return result.IsSuccess
            ? Ok(new { message = result.Message })
            : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpGet("{id:int}/bookings")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetBookings(
        int id,
        CancellationToken cancellationToken)
    {
        var organizerName = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(organizerName))
        {
            return Unauthorized(new { message = "The organizer login session is invalid." });
        }

        var result = await _partnerBookingService.GetBookingsAsync(
            id,
            organizerName,
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Bookings)
            : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("{id:int}/purchase")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Purchase(
        int id,
        PartnerTicketPurchaseRequestDto request,
        CancellationToken cancellationToken)
    {
        request.CustomerName = User.FindFirstValue(ClaimTypes.Name)?.Trim() ?? "KMC Participant";
        request.CustomerEmail = User.FindFirstValue(ClaimTypes.Email)?.Trim().ToLowerInvariant() ?? "participant@kmc.local";
        request.CustomerPhone = "Not provided";

        var result = await _partnerBookingService.PurchaseAsync(
            id,
            request,
            cancellationToken);

        return result.IsSuccess && result.Booking is not null
            ? Ok(result.Booking)
            : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpGet("bookings/{bookingReference}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBooking(
        string bookingReference,
        CancellationToken cancellationToken)
    {
        var booking = await _partnerBookingService.GetBookingAsync(
            bookingReference,
            cancellationToken);

        return booking is null
            ? NotFound(new { message = "Partner booking was not found." })
            : Ok(booking);
    }

    [HttpDelete("bookings/{bookingReference}")]
    [AllowAnonymous]
    public async Task<IActionResult> CancelBooking(
        string bookingReference,
        CancellationToken cancellationToken)
    {
        var result = await _partnerBookingService.CancelBookingAsync(
            bookingReference,
            cancellationToken);

        return result.IsSuccess && result.Booking is not null
            ? Ok(result.Booking)
            : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("sync")]
    [AllowAnonymous]
    public async Task<IActionResult> Sync(
        PartnerEventSyncRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!HasValidApiKey())
        {
            return Unauthorized(new { message = "A valid partner API key is required." });
        }

        try
        {
            return Ok(await _partnerEventService.SyncAsync(request, cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("sync/{externalEventCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteSyncedEvent(
        string externalEventCode,
        CancellationToken cancellationToken)
    {
        if (!HasValidApiKey())
        {
            return Unauthorized(new { message = "A valid partner API key is required." });
        }

        var deleted = await _partnerEventService.DeleteByExternalCodeAsync(
            externalEventCode,
            cancellationToken);

        return deleted
            ? NoContent()
            : NotFound(new { message = "Partner event was not found." });
    }

    private bool HasValidApiKey()
    {
        var expectedKey = _configuration["PartnerIntegration:ApiKey"];
        var providedKey = Request.Headers[ApiKeyHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(expectedKey) ||
            string.IsNullOrWhiteSpace(providedKey))
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expectedKey);
        var providedBytes = Encoding.UTF8.GetBytes(providedKey);

        return expectedBytes.Length == providedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}
