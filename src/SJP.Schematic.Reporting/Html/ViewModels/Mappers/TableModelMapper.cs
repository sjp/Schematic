using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class TableModelMapper
{
    public TableModelMapper(IEnumerable<Identifier> tableNames)
    {
        ArgumentNullException.ThrowIfNull(tableNames);

        TableNames = tableNames.ToHashSet(IdentifierComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The tables the report covers, so that a partition or history table is only linked when there
    /// is a page to link to.
    /// </summary>
    private IReadOnlySet<Identifier> TableNames { get; }

    /// <summary>
    /// Maps a table to its detail payload.
    /// </summary>
    /// <param name="table">The table to map.</param>
    /// <param name="userDefinedTypeTargets">Resolves a column's declared type to the page of the user-defined type it names.</param>
    /// <exception cref="ArgumentNullException"><paramref name="table"/> or <paramref name="userDefinedTypeTargets"/> is <see langword="null" />.</exception>
    public Table Map(IRelationalDatabaseTable table, UserDefinedTypeTargets userDefinedTypeTargets)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(userDefinedTypeTargets);

        var tableColumns = table.Columns.Select(static (c, i) => new { Column = c, Ordinal = i + 1 }).ToList();
        var primaryKey = table.PrimaryKey;
        var uniqueKeys = table.UniqueKeys.ToList();
        var parentKeys = table.ParentKeys.ToList();
        var childKeys = table.ChildKeys.ToList();
        var checks = table.Checks.ToList();
        var triggers = table.Triggers.ToList();

        var keyColumns = table.GetKeyColumns();

        // Foreign keys pair columns by position, so each column links to the column at the same position
        // on the other side of every key it is part of. Links keep key order, then position order.
        var parentKeyLinks = new Dictionary<string, List<KeyColumnLink>>(StringComparer.Ordinal);
        foreach (var parentKey in parentKeys)
            AddKeyColumnLinks(parentKeyLinks, parentKey, parentKey.ChildKey, parentKey.ParentTable, parentKey.ParentKey);

        var childKeyLinks = new Dictionary<string, List<KeyColumnLink>>(StringComparer.Ordinal);
        foreach (var childKey in childKeys)
            AddKeyColumnLinks(childKeyLinks, childKey, childKey.ParentKey, childKey.ChildTable, childKey.ChildKey);

        var visibleTableName = table.Name.ToVisibleName();

        var columns = new List<Table.Column>(tableColumns.Count);
        foreach (var tableColumn in tableColumns)
        {
            var col = tableColumn.Column;
            var columnName = col.Name.LocalName;

            parentKeyLinks.TryGetValue(columnName, out var parentLinks);
            childKeyLinks.TryGetValue(columnName, out var childLinks);

            var qualifiedColumnName = parentLinks != null || childLinks != null
                ? visibleTableName + "." + columnName
                : string.Empty;

            IEnumerable<Table.ParentKey> columnParentKeys = parentLinks != null
                ? parentLinks.ConvertAll(link => new Table.ParentKey(link.ConstraintName, link.TableName, link.ColumnName, qualifiedColumnName))
                : [];
            IEnumerable<Table.ChildKey> columnChildKeys = childLinks != null
                ? childLinks.ConvertAll(link => new Table.ChildKey(link.ConstraintName, link.TableName, link.ColumnName, qualifiedColumnName))
                : [];

            var column = new Table.Column(
                columnName,
                tableColumn.Ordinal,
                tableColumn.Column.IsNullable,
                tableColumn.Column.Type.Definition,
                userDefinedTypeTargets.GetTypeUrl(tableColumn.Column.Type),
                tableColumn.Column.DefaultValue,
                keyColumns.PrimaryKeyColumns.Contains(columnName),
                keyColumns.UniqueKeyColumns.Contains(columnName),
                keyColumns.ForeignKeyColumns.Contains(columnName),
                columnChildKeys,
                columnParentKeys,
                tableColumn.Column.AutoIncrement,
                tableColumn.Column.IsComputed,
                tableColumn.Column.ComputedDefinition,
                tableColumn.Column.ComputedStorage,
                tableColumn.Column.IsHidden
            );
            columns.Add(column);
        }

        var tableIndexes = table.Indexes.ToList();
        var renderIndexes = tableIndexes.ConvertAll(index =>
            new Table.Index(
                index.Name?.LocalName,
                index.IsUnique,
                index.Columns.Select(static c => c.Expression).ToList(),
                index.Columns.Select(static c => c.Order).ToList(),
                index.IncludedColumns.Select(static c => c.Name.LocalName).ToList(),
                index.IndexType,
                index.FilterDefinition,
                index.IsEnabled,
                index.IsValid,
                index.IsVisible
            ));

        var renderPrimaryKey = primaryKey
            .Map(pk => new Table.PrimaryKeyConstraint(
                pk.Name.Match(static name => name.LocalName, static () => string.Empty),
                pk.Columns.Select(static c => c.Name.LocalName).ToList(),
                pk.IsValidated,
                pk.Deferrability
            ));

        var renderUniqueKeys = uniqueKeys
            .ConvertAll(uk => new Table.UniqueKey(
                uk.Name.Match(static name => name.LocalName, static () => string.Empty),
                uk.Columns.Select(static c => c.Name.LocalName).ToList(),
                uk.IsValidated,
                uk.Deferrability
            ));

        var renderParentKeys = parentKeys.ConvertAll(pk =>
            new Table.ForeignKey(
                pk.ChildKey.Name.Match(static name => name.LocalName, static () => string.Empty),
                pk.ChildKey.Columns.Select(static c => c.Name.LocalName).ToList(),
                pk.ParentTable,
                pk.ParentKey.Name.Match(static name => name.LocalName, static () => string.Empty),
                pk.ParentKey.Columns.Select(static c => c.Name.LocalName).ToList(),
                pk.DeleteAction,
                pk.UpdateAction,
                pk.ChildKey.IsValidated,
                pk.ChildKey.Deferrability,
                pk.MatchType
            ));

        var renderChecks = checks.ConvertAll(c =>
            new Table.CheckConstraint(
                c.Name.Match(static name => name.LocalName, static () => string.Empty),
                c.Definition,
                c.IsValidated,
                c.Deferrability
            ));

        var renderTriggers = triggers.ConvertAll(tr =>
            new Table.Trigger(
                tr.Name,
                tr.Definition,
                tr.QueryTiming,
                tr.TriggerEvent,
                tr.Granularity,
                tr.Condition,
                tr.UpdateColumns
            ));

        return new Table(
            table.Name,
            columns,
            renderPrimaryKey,
            renderUniqueKeys,
            renderParentKeys,
            renderChecks,
            renderIndexes,
            renderTriggers,
            table.Kind,
            table.Partitioning.Map(MapPartitioning),
            table.SystemVersioning.Map(MapSystemVersioning),
            table.IsLogged,
            table.Collation
        );
    }

    private static void AddKeyColumnLinks(
        Dictionary<string, List<KeyColumnLink>> links,
        IDatabaseRelationalKey relationalKey,
        IDatabaseKey localKey,
        Identifier linkedTableName,
        IDatabaseKey linkedKey)
    {
        var constraintName = relationalKey.ChildKey.Name.Match(static name => name.LocalName, static () => string.Empty);

        // A column listed more than once in the key gets one link per position. Positions without a
        // counterpart on the other side are skipped.
        using var linkedColumns = linkedKey.Columns.GetEnumerator();
        foreach (var localColumn in localKey.Columns)
        {
            if (!linkedColumns.MoveNext())
                break;

            var localColumnName = localColumn.Name.LocalName;
            if (!links.TryGetValue(localColumnName, out var columnLinks))
            {
                columnLinks = [];
                links.Add(localColumnName, columnLinks);
            }

            columnLinks.Add(new KeyColumnLink(constraintName, linkedTableName, linkedColumns.Current.Name.LocalName));
        }
    }

    private Table.Partitioning MapPartitioning(ITablePartitioning partitioning)
    {
        return new Table.Partitioning(
            partitioning.Strategy,
            partitioning.Columns.Select(static c => c.Name.LocalName).ToList(),
            partitioning.Partitions.Select(MapLinkedTable).ToList()
        );
    }

    private Table.SystemVersioning MapSystemVersioning(ITableSystemVersioning systemVersioning)
    {
        return new Table.SystemVersioning(
            MapLinkedTable(systemVersioning.HistoryTable),
            systemVersioning.PeriodStartColumn.LocalName,
            systemVersioning.PeriodEndColumn.LocalName
        );
    }

    private Table.LinkedTable MapLinkedTable(Identifier name) => new(name, TableNames.Contains(name));

    private readonly record struct KeyColumnLink(string ConstraintName, Identifier TableName, string ColumnName);
}