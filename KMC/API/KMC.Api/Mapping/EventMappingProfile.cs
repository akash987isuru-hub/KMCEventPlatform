using AutoMapper;
using KMC.Api.DTOs.Events;
using KMC.Api.Entities;

namespace KMC.Api.Mapping;

public class EventMappingProfile : Profile
{
    public EventMappingProfile()
    {
        CreateMap<CreateEventRequestDto, Event>()
            .ForMember(
                destination => destination.TicketTiers,
                option => option.Ignore())
            .ForMember(
                destination => destination.Capacity,
                option => option.Ignore());

        CreateMap<UpdateEventRequestDto, Event>()
            .ForMember(
                destination => destination.TicketTiers,
                option => option.Ignore())
            .ForMember(
                destination => destination.Capacity,
                option => option.Ignore());

        CreateMap<TicketTier, TicketTierResponseDto>()
            .ForMember(
                destination => destination.SoldCount,
                option => option.MapFrom(
                    source => source.Registrations
                        .Where(registration =>
                            registration.Status ==
                            RegistrationStatus.Confirmed)
                        .Sum(registration => registration.Quantity)));

        CreateMap<Event, EventResponseDto>()
            .ForMember(
                destination => destination.Status,
                option => option.MapFrom(
                    source => source.Status.ToString()))
            .ForMember(
                destination => destination.OrganizerName,
                option => option.MapFrom(
                    source => source.Organizer.FullName))
            .ForMember(
                destination => destination.RegisteredParticipantCount,
                option => option.MapFrom(
                    source => source.Registrations
                        .Where(registration =>
                            registration.Status ==
                            RegistrationStatus.Confirmed)
                        .Sum(registration => registration.Quantity)))
            .ForMember(
                destination => destination.TicketTiers,
                option => option.MapFrom(
                    source => source.TicketTiers
                        .OrderBy(ticketTier => ticketTier.SortOrder)));
    }
}
