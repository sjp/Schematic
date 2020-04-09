using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class RelationalDatabaseTableProfile : Profile
    {
        public RelationalDatabaseTableProfile()
        {
            CreateMap<Dto.RelationalDatabaseTable, RelationalDatabaseTable>();
            CreateMap<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>();
        }
    }
}
