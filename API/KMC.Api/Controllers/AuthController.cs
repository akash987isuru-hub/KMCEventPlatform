using KMC.Api.DTOs.Auth;
using KMC.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new
            {
                message = "Full name is required."
            });
        }

        if (!request.Role.HasValue ||
            !Enum.IsDefined(request.Role.Value))
        {
            return BadRequest(new
            {
                message =
                    "Role must be either Organizer or Participant."
            });
        }

        var result = await _authService.RegisterAsync(
            request,
            cancellationToken);

        if (result is null)
        {
            return Conflict(new
            {
                message =
                    "An account with this email already exists."
            });
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(
            request,
            cancellationToken);

        if (result is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email address or password."
            });
        }

        return Ok(result);
    }
}
