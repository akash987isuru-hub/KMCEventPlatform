using System.Security.Claims;
using KMC.Api.DTOs.Registrations;
using KMC.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationsController(
        IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    [HttpPost("events/{eventId:int}")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> RegisterForEvent(
        int eventId,
        PurchaseTicketRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var participantId))
        {
            return Unauthorized();
        }

        var result =
            await _registrationService.RegisterForEventAsync(
                eventId,
                participantId,
                request,
                cancellationToken);

        return result.Status switch
        {
            RegistrationOperationStatus.Success =>
                StatusCode(
                    StatusCodes.Status201Created,
                    result.Data),

            RegistrationOperationStatus.Invalid =>
                BadRequest(new
                {
                    message = result.Message
                }),

            RegistrationOperationStatus.NotFound =>
                NotFound(new
                {
                    message = result.Message
                }),

            RegistrationOperationStatus.Forbidden =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = result.Message
                    }),

            RegistrationOperationStatus.Conflict =>
                Conflict(new
                {
                    message = result.Message
                }),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "An unexpected error occurred."
                })
        };
    }

    [HttpGet("my-registrations")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyRegistrations(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var participantId))
        {
            return Unauthorized();
        }

        var registrations =
            await _registrationService.GetMyRegistrationsAsync(
                participantId,
                cancellationToken);

        return Ok(registrations);
    }

    [HttpGet("events/{eventId:int}/participants")]
    [Authorize(Roles = "Organizer")]
    public async Task<IActionResult> GetEventParticipants(
        int eventId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var organizerId))
        {
            return Unauthorized();
        }

        var result =
            await _registrationService.GetEventParticipantsAsync(
                eventId,
                organizerId,
                cancellationToken);

        return result.Status switch
        {
            RegistrationOperationStatus.Success =>
                Ok(result.Participants),

            RegistrationOperationStatus.NotFound =>
                NotFound(new
                {
                    message = result.Message
                }),

            RegistrationOperationStatus.Forbidden =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = result.Message
                    }),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "An unexpected error occurred."
                })
        };
    }

    [HttpDelete("{registrationId:int}")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CancelRegistration(
        int registrationId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var participantId))
        {
            return Unauthorized();
        }

        var result =
            await _registrationService.CancelRegistrationAsync(
                registrationId,
                participantId,
                cancellationToken);

        return result.Status switch
        {
            RegistrationOperationStatus.Success =>
                Ok(new
                {
                    message = result.Message,
                    registration = result.Data
                }),

            RegistrationOperationStatus.Invalid =>
                BadRequest(new
                {
                    message = result.Message
                }),

            RegistrationOperationStatus.NotFound =>
                NotFound(new
                {
                    message = result.Message
                }),

            RegistrationOperationStatus.Forbidden =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        message = result.Message
                    }),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = "An unexpected error occurred."
                })
        };
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.TryParse(userIdValue, out userId);
    }
}
