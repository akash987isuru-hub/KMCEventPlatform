namespace KMC.Api.DTOs.Registrations;

public enum RegistrationOperationStatus
{
    Success,
    Invalid,
    NotFound,
    Forbidden,
    Conflict
}

public class RegistrationOperationResult
{
    public RegistrationOperationStatus Status { get; set; }

    public string? Message { get; set; }

    public RegistrationResponseDto? Data { get; set; }
}