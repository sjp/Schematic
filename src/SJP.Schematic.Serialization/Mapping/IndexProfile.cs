using System.Collections.Generic;
using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class IndexProfile : Profile
{
    public IndexProfile()
    {
        CreateMap<Dto.DatabaseIndex, DatabaseIndex>()
            .ConstructUsing(static (dto, ctx) => new DatabaseIndex(
                ctx.Mapper.Map<Dto.Identifier, Identifier>(dto.IndexName!),
                dto.IsUnique,
                ctx.Mapper.Map<IEnumerable<Dto.DatabaseIndexColumn>, List<DatabaseIndexColumn>>(dto.Columns),
                ctx.Mapper.Map<IEnumerable<Dto.DatabaseColumn>, List<DatabaseColumn>>(dto.IncludedColumns),
                dto.IsEnabled
            ))
            .ForAllMembers(static cfg => cfg.Ignore());
        CreateMap<IDatabaseIndex, Dto.DatabaseIndex>()
            .ForMember(static dest => dest.IndexName, static src => src.MapFrom(static i => i.Name));
    }
}
