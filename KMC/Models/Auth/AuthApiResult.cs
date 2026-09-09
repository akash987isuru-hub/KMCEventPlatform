namespace KMC.Web.Models.Auth;

public class AuthApiResult
{
    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public AuthResponseViewModel? User { get; set; }

    public static AuthApiResult Success(
        AuthResponseViewModel user)
    {
        return new AuthApiResult
        {
            IsSuccess = true,
            User = user
        };
    }

    public static AuthApiResult Failure(string message)
    {
        return new AuthApiResult
        {
            IsSuccess = false,
            ErrorMessage = message
        };
    }
}