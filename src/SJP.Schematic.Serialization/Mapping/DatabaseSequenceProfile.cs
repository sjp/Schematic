using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class DatabaseSequenceProfile : Profile
    {
        public DatabaseSequenceProfile()
        {
            CreateMap<Dto.DatabaseSequence, DatabaseSequence>();
            CreateMap<IDatabaseSequence, Dto.DatabaseSequence>();
        }
    }
}
