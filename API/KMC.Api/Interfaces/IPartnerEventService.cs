using KMC.Api.DTOs.PartnerEvents;

namespace KMC.Api.Interfaces;

public interface IPartnerEventService
{
    Task<IReadOnlyList<PartnerEventResponseDto>> GetAllAsync(
        PartnerEventSearchRequestDto search,
        CancellationToken cancellationToken = default);

    Task<PartnerEventResponseDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PartnerEventResponseDto>> GetManagedAsync(
        string organizerName,
        CancellationToken cancellationToken = default);

    Task<PartnerEventResponseDto?> GetManagedByIdAsync(
        int id,
        string organizerName,
        CancellationToken cancellationToken = default);

    Task<PartnerEventResponseDto> SyncAsync(
        PartnerEventSyncRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PartnerEventManagementResult> UpdateManagedAsync(
        int id,
        PartnerEventUpdateRequestDto request,
        string organizerName,
        CancellationToken cancellationToken = default);

    Task<PartnerEventManagementResult> DeleteManagedAsync(
        int id,
        string organizerName,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteByExternalCodeAsync(
        string externalEventCode,
        CancellationToken cancellationToken = default);
}
