using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseSynonymProfile : Profile
    {
        public DatabaseSynonymProfile()
        {
            CreateMap<Dto.DatabaseSynonym, DatabaseSynonym>();
            CreateMap<IDatabaseSynonym, Dto.DatabaseSynonym>();
        }
    }
}
