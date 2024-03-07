using AutoMapper;
using webapi.DTO;
using webapi.Models;

namespace webapi.Mapper
{
    public class NotifyDtoToNotificationModelMapping : Profile
    {
        public NotifyDtoToNotificationModelMapping()
        {
            CreateMap<NotifyDTO, NotificationModel>();
        }
    }
}
