using KMC.Web.Models.Auth;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMC.Web.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly IKmcApiClient _apiClient;

    public RegisterModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public RegisterViewModel Input { get; set; } = new();

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
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

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.Role is not ("Organizer" or "Participant"))
        {
            ModelState.AddModelError(
                "Input.Role",
                "Select a valid account type.");

            return Page();
        }

        try
        {
            var result = await _apiClient.RegisterAsync(
                Input,
                cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(
                    string.Empty,
                    result.ErrorMessage ?? "Registration failed.");

                return Page();
            }

            TempData["SuccessMessage"] =
                "Your account was created successfully. Please sign in.";

            return RedirectToPage("/Account/Login");
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
                "The registration request took too long to complete.");

            return Page();
        }
    }
}
