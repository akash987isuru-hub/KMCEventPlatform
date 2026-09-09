using System.Text.Json.Serialization;

namespace KMC.Api.DTOs.Auth;

[JsonConverter(
    typeof(JsonStringEnumConverter<PublicRegistrationRole>))]
public enum PublicRegistrationRole
{
    Organizer,
    Participant
}