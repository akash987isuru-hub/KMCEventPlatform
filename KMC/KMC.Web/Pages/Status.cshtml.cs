using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KMC.Web.Pages;

public class StatusModel : PageModel
{
    public int StatusCodeValue { get; private set; }

    public string StatusTitle { get; private set; } =
        "Request could not be completed";

    public void OnGet(int code)
    {
        if (code < 400 || code > 599)
        {
            code = StatusCodes.Status500InternalServerError;
        }

        Response.StatusCode = code;
        StatusCodeValue = code;
        StatusTitle = code switch
        {
            StatusCodes.Status404NotFound => "Page not found",
            StatusCodes.Status403Forbidden => "Access denied",
            StatusCodes.Status401Unauthorized => "Authentication required",
            StatusCodes.Status503ServiceUnavailable => "Service unavailable",
            _ => "Request could not be completed"
        };
    }
}
