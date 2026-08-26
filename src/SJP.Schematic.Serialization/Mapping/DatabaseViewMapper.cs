using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database view between its core and serialized representations.
/// </summary>
public class DatabaseViewMapper
    : IImmutableMapper<Dto.DatabaseView, IDatabaseView>
    , IImmutableMapper<IDatabaseView, Dto.DatabaseView>
{
    /// <summary>
    /// Maps a serialized view to its core representation.
    /// </summary>
    /// <param name="source">A serialized view.</param>
    /// <returns>A view. A materialized view is returned when the serialized view is marked as materialized.</returns>
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

    /// <summary>
    /// Maps a view to its serialized representation.
    /// </summary>
    /// <param name="source">A view.</param>
    /// <returns>A serialized view.</returns>
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
            IsMaterialized = source.IsMaterialized,
        };
    }
}