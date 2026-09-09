using KMC.Web.Pages;
using KMC.Web.Infrastructure;
using KMC.Web.Models.Registrations;
using KMC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KMC.Web.Pages.Registrations;

[Authorize(Roles = "Participant")]
public class IndexModel : AuthenticatedPageModel
{
    private readonly IKmcApiClient _apiClient;

    public IndexModel(IKmcApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public IReadOnlyList<RegistrationViewModel> Registrations
        { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(
        CancellationToken cancellationToken)
    {
        var token = GetJwtToken();

        if (token is null)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Registrations/Index") ?? "/Registrations/Index");
        }

        try
        {
            Registrations =
                (await _apiClient.GetMyRegistrationsAsync(
                    token,
                    cancellationToken))
                .Where(registration => !string.Equals(
                    registration.Status,
                    "Cancelled",
                    StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(
                        registration.Status,
                        "Canceled",
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(registration => registration.EventDate)
                .ThenBy(registration => registration.StartTime)
                .ToList();
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Registrations/Index") ?? "/Registrations/Index");
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "The registration service is currently unavailable.";
        }
        catch (TaskCanceledException)
        {
            ErrorMessage =
                "The registration request took too long to complete.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(
        int registrationId,
        CancellationToken cancellationToken)
    {
        if (registrationId <= 0)
        {
            TempData["RegistrationError"] =
                "Invalid registration selected.";

            return RedirectToPage();
        }

        var token = GetJwtToken();

        if (token is null)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Registrations/Index") ?? "/Registrations/Index");
        }

        try
        {
            var result = await _apiClient.CancelRegistrationAsync(
                registrationId,
                token,
                cancellationToken);

            TempData[result.IsSuccess
                ? "RegistrationSuccess"
                : "RegistrationError"] =
                result.Message ??
                (result.IsSuccess
                    ? "Registration cancelled successfully."
                    : "Registration could not be cancelled.");
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Registrations/Index") ?? "/Registrations/Index");
        }
        catch (HttpRequestException)
        {
            TempData["RegistrationError"] =
                "The registration service is currently unavailable.";
        }
        catch (TaskCanceledException)
        {
            TempData["RegistrationError"] =
                "The cancellation request took too long to complete.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetDownloadReceiptAsync(
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
                Url.Page("/Registrations/Index") ?? "/Registrations/Index");
        }

        try
        {
            var registrations = await _apiClient.GetMyRegistrationsAsync(
                token,
                cancellationToken);

            var registration = registrations.FirstOrDefault(
                item => item.Id == registrationId);

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
                    "Only confirmed payment receipts can be downloaded.";

                return RedirectToPage();
            }

            var pdf = ReceiptPdfGenerator.Generate(registration);
            var fileName = $"KMC-Payment-Receipt-{registration.Id:000000}.pdf";

            return File(pdf, "application/pdf", fileName);
        }
        catch (UnauthorizedAccessException)
        {
            return await RedirectToLoginAsync(
                Url.Page("/Registrations/Index") ?? "/Registrations/Index");
        }
        catch (HttpRequestException)
        {
            TempData["RegistrationError"] =
                "The receipt could not be downloaded because the service is unavailable.";
        }
        catch (TaskCanceledException)
        {
            TempData["RegistrationError"] =
                "The receipt download request took too long to complete.";
        }

        return RedirectToPage();
    }
}
