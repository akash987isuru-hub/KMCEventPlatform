using KMC.Api.DTOs.Auth;

namespace KMC.Api.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AuthResponseDto?> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);
}