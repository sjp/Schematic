using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

public class DatabaseViewMapper
    : IImmutableMapper<Dto.DatabaseView, IDatabaseView>
    , IImmutableMapper<IDatabaseView, Dto.DatabaseView>
{
    public IDatabaseView Map(Dto.DatabaseView source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();

        var viewName = identifierMapper.Map(source.ViewName);
        var columns = columnMapper.MapList(source.Columns);

        return source.IsMaterialized
            ? new DatabaseMaterializedView(viewName, source.Definition, columns)
            : new DatabaseView(viewName, source.Definition, columns);
    }

    public Dto.DatabaseView Map(IDatabaseView source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var columnMapper = MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();

        var viewName = identifierMapper.Map(source.Name);
        var columns = columnMapper.MapList(source.Columns);

        return new Dto.DatabaseView
        {
            ViewName = viewName,
            Columns = columns,
            Definition = source.Definition,
            IsMaterialized = source.IsMaterialized
        };
    }
}