using KMC.Api.DTOs.PartnerEvents;
using KMC.Api.Entities;

namespace KMC.Api.Interfaces;

public interface IOrganizerPartnerEventClient
{
    Task<PartnerEventManagementResult> UpdateAsync(
        PartnerEvent eventItem,
        PartnerEventUpdateRequestDto request,
        CancellationToken cancellationToken = default);

    Task<PartnerEventManagementResult> DeleteAsync(
        PartnerEvent eventItem,
        CancellationToken cancellationToken = default);
}
