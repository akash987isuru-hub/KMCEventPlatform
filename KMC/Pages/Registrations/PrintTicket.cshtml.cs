using KMC.Web.Models.Registrations;
using KMC.Web.Pages;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Registrations;

[Authorize(Roles = "Participant")]
public class PrintTicketModel : AuthenticatedPageModel
{
    private readonly IKmcApiClient _apiClient;

    public PrintTicketModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public RegistrationViewModel Registration { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(
        int registrationId,
        CancellationToken cancellationToken)
    {
        if (registrationId <= 0)
        {
            return BadRequest();
        }

        var token = GetJwtToken();
        if (token is null)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Registrations/PrintTicket", new { registrationId }) ??
                "/Registrations/Index");
        }

        try
        {
            var registrations = await _apiClient.GetMyRegistrationsAsync(
                token,
                cancellationToken);

            var registration = registrations.FirstOrDefault(item =>
                item.Id == registrationId);

            if (registration is null)
            {
                return NotFound();
            }

            if (!string.Equals(
                    registration.Status,
                    "Confirmed",
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["RegistrationError"] =
                    "Only confirmed tickets can be printed.";
                return RedirectToPage("/Registrations/Index");
            }

            Registration = registration;
            return Page();
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Registrations/PrintTicket", new { registrationId }) ??
                "/Registrations/Index");
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException)
        {
            TempData["RegistrationError"] =
                "The ticket could not be prepared because the service is unavailable.";
            return RedirectToPage("/Registrations/Index");
        }
    }
}
