using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class IdentifierDefaultsProfile : Profile
    {
        public IdentifierDefaultsProfile()
        {
            CreateMap<Dto.IdentifierDefaults, IdentifierDefaults>();
            CreateMap<IIdentifierDefaults, Dto.IdentifierDefaults>();
        }
    }
}
