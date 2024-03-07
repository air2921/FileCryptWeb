using AutoMapper;
using webapi.DTO;
using webapi.Models;

namespace webapi.Mapper
{
    public class StorageDtoToKeyStorageModelMapping : Profile
    {
        public StorageDtoToKeyStorageModelMapping()
        {
            CreateMap<StorageDTO, KeyStorageModel>();
        }
    }
}
