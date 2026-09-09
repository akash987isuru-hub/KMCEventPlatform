using Organizer.Api.DTOs.Kmc;
using Organizer.Api.Entities;

namespace Organizer.Api.Interfaces;

public interface IKmcPartnerEventClient
{
    Task<KmcPublishResult> SyncAsync(
        OrganizerEvent eventItem,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string externalEventCode,
        CancellationToken cancellationToken = default);
}