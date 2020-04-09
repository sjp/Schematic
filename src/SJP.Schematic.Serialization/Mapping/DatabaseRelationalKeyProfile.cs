using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseRelationalKeyProfile : Profile
    {
        public DatabaseRelationalKeyProfile()
        {
            CreateMap<Dto.DatabaseRelationalKey, DatabaseRelationalKey>();
            CreateMap<IDatabaseRelationalKey, Dto.DatabaseRelationalKey>();
        }
    }
}
