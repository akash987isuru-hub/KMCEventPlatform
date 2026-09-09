using System.Text.Json.Serialization;

namespace KMC.Api.Entities;

[JsonConverter(
    typeof(JsonStringEnumConverter<EventStatus>))]
public enum EventStatus
{
    Draft,
    Published,
    Cancelled,
    Completed
}