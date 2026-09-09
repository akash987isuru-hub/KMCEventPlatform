namespace KMC.Web.Models.Registrations;

public class RegistrationApiResult
{
    public bool IsSuccess { get; set; }

    public string? Message { get; set; }

    public RegistrationViewModel? Registration { get; set; }

    public static RegistrationApiResult Success(
        RegistrationViewModel registration)
    {
        return new RegistrationApiResult
        {
            IsSuccess = true,
            Message = "Event registration completed successfully.",
            Registration = registration
        };
    }

    public static RegistrationApiResult Failure(string message)
    {
        return new RegistrationApiResult
        {
            IsSuccess = false,
            Message = message
        };
    }
}