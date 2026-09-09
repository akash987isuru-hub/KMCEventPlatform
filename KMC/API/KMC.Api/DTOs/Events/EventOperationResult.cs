namespace KMC.Api.DTOs.Events;

public enum EventOperationStatus
{
    Success,
    Invalid,
    NotFound,
    Forbidden
}

public class EventOperationResult
{
    public EventOperationStatus Status { get; set; }

    public string? Message { get; set; }

    public EventResponseDto? Data { get; set; }
}