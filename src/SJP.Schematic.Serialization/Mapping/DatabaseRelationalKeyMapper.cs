using System.Linq;
using Boxed.Mapping;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a foreign key relationship between its core and serialized representations.
/// </summary>
public class DatabaseRelationalKeyMapper
    : IImmutableMapper<Dto.DatabaseRelationalKey, IDatabaseRelationalKey>
    , IImmutableMapper<IDatabaseRelationalKey, Dto.DatabaseRelationalKey>
{
    /// <summary>
    /// Maps a serialized foreign key relationship to its core representation.
    /// </summary>
    /// <param name="source">A serialized foreign key relationship.</param>
    /// <returns>A foreign key relationship.</returns>
    public IDatabaseRelationalKey Map(Dto.DatabaseRelationalKey source) => Map(source, ColumnLookup.Empty, ColumnLookup.Empty);

    internal IDatabaseRelationalKey Map(Dto.DatabaseRelationalKey source, ColumnLookup childTableColumns, ColumnLookup parentTableColumns)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var databaseKeyMapper = (DatabaseKeyMapper)MapperRegistry.GetMapper<Dto.DatabaseKey, IDatabaseKey>();

        return new DatabaseRelationalKey(
            identifierMapper.Map(source.ChildTable),
            databaseKeyMapper.Map(source.ChildKey, childTableColumns),
            identifierMapper.Map(source.ParentTable),
            databaseKeyMapper.Map(source.ParentKey, parentTableColumns),
            source.DeleteAction,
            source.UpdateAction,
            source.MatchType,
            childTableColumns.ResolveList(source.SetNullColumns)
        );
    }

    /// <summary>
    /// Maps a foreign key relationship to its serialized representation.
    /// </summary>
    /// <param name="source">A foreign key relationship.</param>
    /// <returns>A serialized foreign key relationship.</returns>
    public Dto.DatabaseRelationalKey Map(IDatabaseRelationalKey source) => Map(source, new SerializedObjectCache());

    internal Dto.DatabaseRelationalKey Map(IDatabaseRelationalKey source, SerializedObjectCache cache)
    {
        if (cache.RelationalKeys.TryGetValue(source, out var cached))
            return cached;

        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var databaseKeyMapper = (DatabaseKeyMapper)MapperRegistry.GetMapper<IDatabaseKey, Dto.DatabaseKey>();
        var columnMapper = (DatabaseColumnMapper)MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();

        var result = new Dto.DatabaseRelationalKey
        {
            ChildTable = identifierMapper.Map(source.ChildTable),
            ChildKey = databaseKeyMapper.Map(source.ChildKey, cache),
            ParentTable = identifierMapper.Map(source.ParentTable),
            ParentKey = databaseKeyMapper.Map(source.ParentKey, cache),
            DeleteAction = source.DeleteAction,
            UpdateAction = source.UpdateAction,
            MatchType = source.MatchType,
            SetNullColumns = source.SetNullColumns.Select(column => columnMapper.Map(column, cache)).ToList(),
        };

        cache.RelationalKeys.Add(source, result);
        return result;
    }
}