using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseColumnProfile : Profile
    {
        public DatabaseColumnProfile()
        {
            CreateMap<Dto.DatabaseColumn, DatabaseColumn>();
            CreateMap<IDatabaseColumn, Dto.DatabaseColumn>();
        }
    }
}
