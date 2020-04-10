using System.Collections.Generic;
using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    public class IndexProfile : Profile
    {
        public IndexProfile()
        {
            CreateMap<Dto.DatabaseIndex, DatabaseIndex>()
                .ConstructUsing((dto, ctx) => new DatabaseIndex(
                    ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.IndexName!),
                    dto.IsUnique,
                    ctx.Mapper.Map<IEnumerable<Dto.DatabaseIndexColumn>, List<DatabaseIndexColumn>>(dto.Columns),
                    ctx.Mapper.Map<IEnumerable<Dto.DatabaseColumn>, List<DatabaseColumn>>(dto.IncludedColumns),
                    dto.IsEnabled
                ));
            CreateMap<IDatabaseIndex, Dto.DatabaseIndex>()
                .ForMember(dest => dest.IndexName, src => src.MapFrom(i => i.Name));
        }
    }
}
