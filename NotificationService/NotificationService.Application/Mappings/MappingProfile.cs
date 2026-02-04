using AutoMapper;
using NotificationService.Application.Models;
using NotificationService.Application.Models.Events;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateNotificationEvent, CreateNotificationModel>();

        CreateMap<CreateNotificationModel, Notification>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => TimeProvider.System.GetUtcNow()));

        CreateMap<CreateNotificationEvent, CreateNotificationModel>();
    }
}
