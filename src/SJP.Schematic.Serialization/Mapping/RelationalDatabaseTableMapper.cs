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
    /// <remarks>
    /// The columns of the table's keys and indexes, and the columns of its own side of each foreign
    /// key, are the instances in the table's <see cref="IRelationalDatabaseTable.Columns"/> whenever
    /// a column of that name exists. The other table's side of a foreign key is read as separate
    /// instances, because that table is not mapped here.
    /// </remarks>
    public IRelationalDatabaseTable Map(Dto.RelationalDatabaseTable source)
    {
        var columnMapper = MapperRegistry.GetMapper<Dto.DatabaseColumn, IDatabaseColumn>();

        return Map(source, new ColumnLookup(columnMapper.MapList(source.Columns)), EmptyTableColumns);
    }

    /// <summary>
    /// Maps a serialized table whose columns are already mapped.
    /// </summary>
    /// <param name="source">A serialized table.</param>
    /// <param name="columns">The table's mapped columns.</param>
    /// <param name="tableColumns">The mapped columns of the other tables, by table name, used for the other side of foreign keys.</param>
    /// <returns>A table.</returns>
    internal IRelationalDatabaseTable Map(
        Dto.RelationalDatabaseTable source,
        ColumnLookup columns,
        IReadOnlyDictionary<Dto.Identifier, ColumnLookup> tableColumns)
    {
        var identifierMapper = MapperRegistry.GetMapper<Dto.Identifier, Identifier>();
        var keyMapper = (DatabaseKeyMapper)MapperRegistry.GetMapper<Dto.DatabaseKey, IDatabaseKey>();
        var relationalKeyMapper = (DatabaseRelationalKeyMapper)MapperRegistry.GetMapper<Dto.DatabaseRelationalKey, IDatabaseRelationalKey>();
        var indexMapper = (IndexMapper)MapperRegistry.GetMapper<Dto.DatabaseIndex, IDatabaseIndex>();
        var checkMapper = MapperRegistry.GetMapper<Dto.DatabaseCheckConstraint, IDatabaseCheckConstraint>();
        var triggerMapper = MapperRegistry.GetMapper<Dto.DatabaseTrigger, IDatabaseTrigger>();

        ColumnLookup ColumnsOf(Dto.Identifier tableName) => tableName == source.TableName
            ? columns
            : tableColumns.GetValueOrDefault(tableName, ColumnLookup.Empty);

        var primaryKey = source.PrimaryKey != null
            ? Option<IDatabaseKey>.Some(keyMapper.Map(source.PrimaryKey, columns))
            : Option<IDatabaseKey>.None;

        return new RelationalDatabaseTable(
            identifierMapper.Map<Dto.Identifier, Identifier>(source.TableName),
            columns.Columns,
            primaryKey,
            source.UniqueKeys.Select(key => keyMapper.Map(key, columns)).ToList(),
            source.ParentKeys.Select(key => relationalKeyMapper.Map(key, ColumnsOf(key.ChildTable), ColumnsOf(key.ParentTable))).ToList(),
            source.ChildKeys.Select(key => relationalKeyMapper.Map(key, ColumnsOf(key.ChildTable), ColumnsOf(key.ParentTable))).ToList(),
            source.Indexes.Select(index => indexMapper.Map(index, columns)).ToList(),
            checkMapper.MapList(source.Checks),
            triggerMapper.MapList(source.Triggers),
            source.Kind,
            MapPartitioning(source.Partitioning, columns.Columns),
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
    public Dto.RelationalDatabaseTable Map(IRelationalDatabaseTable source) => Map(source, new SerializedObjectCache());

    internal Dto.RelationalDatabaseTable Map(IRelationalDatabaseTable source, SerializedObjectCache cache)
    {
        var identifierMapper = MapperRegistry.GetMapper<Identifier, Dto.Identifier>();
        var columnMapper = (DatabaseColumnMapper)MapperRegistry.GetMapper<IDatabaseColumn, Dto.DatabaseColumn>();
        var keyMapper = (DatabaseKeyMapper)MapperRegistry.GetMapper<IDatabaseKey, Dto.DatabaseKey>();
        var relationalKeyMapper = (DatabaseRelationalKeyMapper)MapperRegistry.GetMapper<IDatabaseRelationalKey, Dto.DatabaseRelationalKey>();
        var indexMapper = (IndexMapper)MapperRegistry.GetMapper<IDatabaseIndex, Dto.DatabaseIndex>();
        var checkMapper = MapperRegistry.GetMapper<IDatabaseCheckConstraint, Dto.DatabaseCheckConstraint>();
        var triggerMapper = MapperRegistry.GetMapper<IDatabaseTrigger, Dto.DatabaseTrigger>();

        return new Dto.RelationalDatabaseTable
        {
            TableName = identifierMapper.Map(source.Name),
            Columns = source.Columns.Select(column => columnMapper.Map(column, cache)).ToList(),
            PrimaryKey = keyMapper.Map(source.PrimaryKey, cache),
            UniqueKeys = source.UniqueKeys.Select(key => keyMapper.Map(key, cache)).ToList(),
            ParentKeys = source.ParentKeys.Select(key => relationalKeyMapper.Map(key, cache)).ToList(),
            ChildKeys = source.ChildKeys.Select(key => relationalKeyMapper.Map(key, cache)).ToList(),
            Indexes = source.Indexes.Select(index => indexMapper.Map(index, cache)).ToList(),
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

    private static readonly IReadOnlyDictionary<Dto.Identifier, ColumnLookup> EmptyTableColumns = new Dictionary<Dto.Identifier, ColumnLookup>();
}