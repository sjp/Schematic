using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class IndexProfile : Profile
    {
        public IndexProfile()
        {
            CreateMap<Dto.DatabaseIndex, DatabaseIndex>();
            CreateMap<IDatabaseIndex, Dto.DatabaseIndex>();
        }
    }
}
