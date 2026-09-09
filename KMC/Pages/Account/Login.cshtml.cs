using System.Security.Claims;
using KMC.Web.Infrastructure;
using KMC.Web.Models.Auth;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMC.Web.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IKmcApiClient _apiClient;

    public LoginModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        string? returnUrl = null,
        bool switchAccount = false)
    {
        if (switchAccount && User.Identity?.IsAuthenticated == true)
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        }
        else if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectAuthenticatedUser();
        }

        Input.ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var result = await _apiClient.LoginAsync(
                Input,
                cancellationToken);

            if (!result.IsSuccess || result.User is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.ErrorMessage ??
                    "Invalid email address or password.");

                return Page();
            }

            await SignInClientUserAsync(result.User);

            HttpContext.Session.SetString(
                SessionKeys.ApiJwtToken,
                result.User.Token);

            if (!string.IsNullOrWhiteSpace(Input.ReturnUrl) &&
                Url.IsLocalUrl(Input.ReturnUrl))
            {
                return LocalRedirect(Input.ReturnUrl);
            }

            return result.User.Role switch
            {
                "Organizer" => RedirectToPage("/Organizer/Home"),
                "Participant" => RedirectToPage("/Participant/Home"),
                _ => RedirectToPage("/Index")
            };
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The authentication service is currently unavailable.");

            return Page();
        }
        catch (TaskCanceledException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The login request took too long to complete.");

            return Page();
        }
    }

    private async Task SignInClientUserAsync(
        AuthResponseViewModel user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var expiresUtc = user.TokenExpiration > DateTime.UtcNow
            ? new DateTimeOffset(
                DateTime.SpecifyKind(
                    user.TokenExpiration,
                    DateTimeKind.Utc))
            : DateTimeOffset.UtcNow.AddHours(1);

        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            AllowRefresh = true,
            ExpiresUtc = expiresUtc
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);
    }

    private IActionResult RedirectAuthenticatedUser()
    {
        if (User.IsInRole("Organizer"))
        {
            return RedirectToPage("/Organizer/Home");
        }

        if (User.IsInRole("Participant"))
        {
            return RedirectToPage("/Participant/Home");
        }

        return RedirectToPage("/Index");
    }
}
