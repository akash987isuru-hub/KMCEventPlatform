using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KMC.Api.Data;
using KMC.Api.DTOs.Auth;
using KMC.Api.Entities;
using KMC.Api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KMC.Api.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext context,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto?> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _context.Users.AnyAsync(
            user => user.Email == normalizedEmail,
            cancellationToken);

        if (emailExists)
        {
            return null;
        }

        var userRole = request.Role switch
        {
            PublicRegistrationRole.Organizer =>
                UserRole.Organizer,

            PublicRegistrationRole.Participant =>
                UserRole.Participant,

            _ => throw new InvalidOperationException(
                "Unsupported public registration role.")
        };

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            Role = userRole,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var duplicateEmail = await _context.Users
                .AsNoTracking()
                .AnyAsync(
                    existingUser =>
                        existingUser.Email == normalizedEmail,
                    cancellationToken);

            if (duplicateEmail)
            {
                return null;
            }

            throw;
        }

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponseDto?> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users.FirstOrDefaultAsync(
            item => item.Email == normalizedEmail,
            cancellationToken);

        if (user is null)
        {
            return null;
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (verificationResult ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            await _context.SaveChangesAsync(cancellationToken);
        }

        return GenerateAuthResponse(user);
    }

    private AuthResponseDto GenerateAuthResponse(User user)
    {
        var jwtSection = _configuration.GetSection("Jwt");

        var key = jwtSection["Key"]
            ?? throw new InvalidOperationException(
                "JWT key is missing from configuration.");

        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is missing from configuration.");

        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is missing from configuration.");

        var expirationMinutes = int.TryParse(
            jwtSection["ExpirationMinutes"],
            out var configuredMinutes)
                ? configuredMinutes
                : 60;

        var expiresAt = DateTime.UtcNow.AddMinutes(
            expirationMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new(
                ClaimTypes.Name,
                user.FullName),

            new(
                ClaimTypes.Email,
                user.Email),

            new(
                ClaimTypes.Role,
                user.Role.ToString()),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler()
            .WriteToken(jwtToken);

        return new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Token = token,
            TokenExpiration = expiresAt
        };
    }
}