using KMC.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMC.Web.Pages;

public abstract class AuthenticatedPageModel : PageModel
{
    protected string? GetJwtToken()
    {
        var token = HttpContext.Session.GetString(SessionKeys.ApiJwtToken);
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }

    protected async Task<IActionResult> RedirectToLoginAsync(
        string returnUrl)
    {
        HttpContext.Session.Clear();

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToPage(
            "/Account/Login",
            new { returnUrl });
    }
}
