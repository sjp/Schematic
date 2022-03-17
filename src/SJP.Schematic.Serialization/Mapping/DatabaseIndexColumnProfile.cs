using System.Collections.Generic;
using AutoMapper;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DatabaseIndexColumnProfile : Profile
{
    public DatabaseIndexColumnProfile()
    {
        CreateMap<Dto.DatabaseIndexColumn, DatabaseIndexColumn>()
            .ConstructUsing(static (dto, ctx) => new DatabaseIndexColumn(
                dto.Expression!,
                ctx.Mapper.Map<IEnumerable<Dto.DatabaseColumn>, IEnumerable<DatabaseColumn>>(dto.DependentColumns),
                dto.Order
            ))
            .ForAllMembers(static cfg => cfg.Ignore());
        CreateMap<IDatabaseIndexColumn, Dto.DatabaseIndexColumn>();
    }
}
