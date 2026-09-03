using Boxed.Mapping;
using LanguageExt;
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
        var triggerMapper = MapperRegistry.GetMapper<Dto.DatabaseTrigger, IDatabaseTrigger>();
        var indexMapper = MapperRegistry.GetMapper<Dto.DatabaseIndex, IDatabaseIndex>();
        var optionalMapper = MapperRegistry.GetMapper<string?, Option<string>>();

        var viewName = identifierMapper.Map(source.ViewName);
        var columns = columnMapper.MapList(source.Columns);
        var triggers = triggerMapper.MapList(source.Triggers);
        var indexes = indexMapper.MapList(source.Indexes);

        return source.IsMaterialized
            ? new DatabaseMaterializedView(
                viewName,
                source.Definition,
                columns,
                triggers,
                indexes,
                source.RefreshMode,
                optionalMapper.Map(source.RefreshMethod),
                source.IsPopulated
            )
            : new DatabaseView(
                viewName,
                source.Definition,
                columns,
                triggers,
                indexes,
                source.CheckOption,
                source.IsUpdatable
            );
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
        var triggerMapper = MapperRegistry.GetMapper<IDatabaseTrigger, Dto.DatabaseTrigger>();
        var indexMapper = MapperRegistry.GetMapper<IDatabaseIndex, Dto.DatabaseIndex>();
        var optionalMapper = MapperRegistry.GetMapper<Option<string>, string?>();

        var viewName = identifierMapper.Map(source.Name);
        var columns = columnMapper.MapList(source.Columns);
        var triggers = triggerMapper.MapList(source.Triggers);
        var indexes = indexMapper.MapList(source.Indexes);

        // the refresh metadata only exists on a materialized view; a view provider that reports a
        // materialized view without implementing the interface leaves it at its default.
        var materializedView = source as IDatabaseMaterializedView;

        return new Dto.DatabaseView
        {
            ViewName = viewName,
            Columns = columns,
            Definition = source.Definition,
            IsMaterialized = source.IsMaterialized,
            Triggers = triggers,
            Indexes = indexes,
            CheckOption = source.CheckOption,
            IsUpdatable = source.IsUpdatable,
            RefreshMode = materializedView?.RefreshMode ?? MaterializedViewRefreshMode.Unknown,
            RefreshMethod = optionalMapper.Map(materializedView?.RefreshMethod ?? Option<string>.None),
            IsPopulated = materializedView?.IsPopulated ?? false,
        };
    }
}
