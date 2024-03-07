using AutoMapper;
using webapi.DTO;
using webapi.Models;

namespace webapi.Mapper
{
    public class KeyDtoToKeyStorageItemModelMapping : Profile
    {
        public KeyDtoToKeyStorageItemModelMapping()
        {
            CreateMap<KeyDTO, KeyStorageItemModel>();
        }
    }
}
