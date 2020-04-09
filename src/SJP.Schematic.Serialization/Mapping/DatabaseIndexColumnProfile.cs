using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseIndexColumnProfile : Profile
    {
        public DatabaseIndexColumnProfile()
        {
            CreateMap<Dto.DatabaseIndexColumn, DatabaseIndexColumn>();
            CreateMap<IDatabaseIndexColumn, Dto.DatabaseIndexColumn>();
        }
    }
}
