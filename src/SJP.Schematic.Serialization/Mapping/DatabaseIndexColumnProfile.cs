using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DatabaseIndexColumnProfile
    : IImmutableMapper<Dto.DatabaseIndexColumn, IDatabaseIndexColumn>
    , IImmutableMapper<IDatabaseIndexColumn, Dto.DatabaseIndexColumn>
{
    public IDatabaseIndexColumn Map(Dto.DatabaseIndexColumn source)
    {
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();

        return new DatabaseIndexColumn(
            source.Expression!,
            columnMapper.MapList(source.DependentColumns),
            source.Order
        );
    }

    public Dto.DatabaseIndexColumn Map(IDatabaseIndexColumn source)
    {
        var columnMapper = MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();

        return new Dto.DatabaseIndexColumn
        {
            Expression = source.Expression!,
            DependentColumns = columnMapper.MapList(source.DependentColumns),
            Order = source.Order
        };
    }
}
