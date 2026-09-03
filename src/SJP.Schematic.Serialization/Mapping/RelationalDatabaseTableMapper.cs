using System.Collections.Generic;
using System.Linq;
using Boxed.Mapping;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Maps a database table between its core and serialized representations.
/// </summary>
public class RelationalDatabaseTableMapper
    : IImmutableMapper<Dto.RelationalDatabaseTable, IRelationalDatabaseTable>
    , IImmutableMapper<IRelationalDatabaseTable, Dto.RelationalDatabaseTable>
{
    /// <summary>
    /// Maps a serialized table to its core representation.
    /// </summary>
    /// <param name="source">A serialized table.</param>
    /// <returns>A table.</returns>
    public IRelationalDatabaseTable Map(Dto.RelationalDatabaseTable source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();
        var optionalKeyMapper = MapperRegistry.GetMapper<Dto.DatabaseKey?, Option<IDatabaseKey>>();
        var keyMapper = MapperRegistry.GetMapper<Dto.DatabaseKey, IDatabaseKey>();
        var relationalKeyMapper = MapperRegistry.GetMapper<Dto.DatabaseRelationalKey, IDatabaseRelationalKey>();
        var indexMapper = MapperRegistry.GetMapper<Dto.DatabaseIndex, IDatabaseIndex>();
        var checkMapper = MapperRegistry.GetMapper<Dto.DatabaseCheckConstraint, IDatabaseCheckConstraint>();
        var triggerMapper = MapperRegistry.GetMapper<Dto.DatabaseTrigger, IDatabaseTrigger>();

        var columns = columnMapper.MapList(source.Columns);

        return new RelationalDatabaseTable(
            identifierMapper.Map<Dto.Identifier, Identifier>(source.TableName),
            columns,
            optionalKeyMapper.Map(source.PrimaryKey),
            keyMapper.MapList(source.UniqueKeys),
            relationalKeyMapper.MapList(source.ParentKeys),
            relationalKeyMapper.MapList(source.ChildKeys),
            indexMapper.MapList(source.Indexes),
            checkMapper.MapList(source.Checks),
            triggerMapper.MapList(source.Triggers),
            source.Kind,
            MapPartitioning(source.Partitioning, columns),
            MapSystemVersioning(source.SystemVersioning),
            source.IsLogged,
            source.Collation == null
                ? Option<Identifier>.None
                : Option<Identifier>.Some(identifierMapper.Map<Dto.Identifier, Identifier>(source.Collation))
        );
    }

    // A partitioning key names the table's own columns, so the mapped columns are looked up rather
    // than mapped a second time; a name that no longer matches a column is dropped.
    private static Option<ITablePartitioning> MapPartitioning(Dto.TablePartitioning? source, IReadOnlyList<IDatabaseColumn> columns)
    {
        if (source == null)
            return Option<ITablePartitioning>.None;

        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var columnLookup = columns.ToDictionary(static column => column.Name, IdentifierComparer.OrdinalIgnoreCase);

        var partitionColumns = source.Columns
            .Select(columnName => columnLookup.TryGetValue(identifierMapper.Map<Dto.Identifier, Identifier>(columnName), out var column) ? column : null)
            .Where(static column => column != null)
            .Select(static column => column!)
            .ToList();
        var partitions = source.Partitions
            .Select(identifierMapper.Map<Dto.Identifier, Identifier>)
            .ToList();

        return Option<ITablePartitioning>.Some(new TablePartitioning(source.Strategy, partitionColumns, partitions));
    }

    private static Option<ITableSystemVersioning> MapSystemVersioning(Dto.TableSystemVersioning? source)
    {
        if (source == null)
            return Option<ITableSystemVersioning>.None;

        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();

        return Option<ITableSystemVersioning>.Some(new TableSystemVersioning(
            identifierMapper.Map<Dto.Identifier, Identifier>(source.HistoryTable),
            identifierMapper.Map<Dto.Identifier, Identifier>(source.PeriodStartColumn),
            identifierMapper.Map<Dto.Identifier, Identifier>(source.PeriodEndColumn)
        ));
    }

    /// <summary>
    /// Maps a table to its serialized representation.
    /// </summary>
    /// <param name="source">A table.</param>
    /// <returns>A serialized table.</returns>
    public Dto.RelationalDatabaseTable Map(IRelationalDatabaseTable source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var columnMapper = MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();
        var optionalKeyMapper = MapperRegistry.GetMapper<Option<IDatabaseKey>, Dto.DatabaseKey?>();
        var keyMapper = MapperRegistry.GetMapper<IDatabaseKey, Dto.DatabaseKey>();
        var relationalKeyMapper = MapperRegistry.GetMapper<IDatabaseRelationalKey, Dto.DatabaseRelationalKey>();
        var indexMapper = MapperRegistry.GetMapper<IDatabaseIndex, Dto.DatabaseIndex>();
        var checkMapper = MapperRegistry.GetMapper<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>();
        var triggerMapper = MapperRegistry.GetMapper<IDatabaseTrigger, Dto.DatabaseTrigger>();

        return new Dto.RelationalDatabaseTable
        {
            TableName = identifierMapper.Map(source.Name),
            Columns = columnMapper.MapList(source.Columns),
            PrimaryKey = optionalKeyMapper.Map(source.PrimaryKey),
            UniqueKeys = keyMapper.MapList(source.UniqueKeys),
            ParentKeys = relationalKeyMapper.MapList(source.ParentKeys),
            ChildKeys = relationalKeyMapper.MapList(source.ChildKeys),
            Indexes = indexMapper.MapList(source.Indexes),
            Checks = checkMapper.MapList(source.Checks),
            Triggers = triggerMapper.MapList(source.Triggers),
            Kind = source.Kind,
            Partitioning = source.Partitioning.MatchUnsafe(MapPartitioning, static () => null),
            SystemVersioning = source.SystemVersioning.MatchUnsafe(MapSystemVersioning, static () => null),
            IsLogged = source.IsLogged,
            Collation = source.Collation.MatchUnsafe(identifierMapper.Map, static () => null),
        };
    }

    private static Dto.TablePartitioning MapPartitioning(ITablePartitioning source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();

        return new Dto.TablePartitioning
        {
            Strategy = source.Strategy,
            Columns = source.Columns.Select(static column => column.Name).Select(identifierMapper.Map).ToList(),
            Partitions = source.Partitions.Select(identifierMapper.Map).ToList(),
        };
    }

    private static Dto.TableSystemVersioning MapSystemVersioning(ITableSystemVersioning source)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();

        return new Dto.TableSystemVersioning
        {
            HistoryTable = identifierMapper.Map(source.HistoryTable),
            PeriodStartColumn = identifierMapper.Map(source.PeriodStartColumn),
            PeriodEndColumn = identifierMapper.Map(source.PeriodEndColumn),
        };
    }
}