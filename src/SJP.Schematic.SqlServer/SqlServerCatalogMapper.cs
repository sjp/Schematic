using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Queries;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// Maps the raw index and trigger rows returned by the SQL Server catalog onto the core model.
/// Indexes and triggers attach to tables and to views in exactly the same shape, so the table and
/// view providers share this mapping rather than each carrying its own copy.
/// </summary>
internal static class SqlServerCatalogMapper
{
    public static IReadOnlyCollection<IDatabaseIndex> MapIndexes(
        IEnumerable<GetTableIndexes.Result> rows,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup,
        IDatabaseDialect dialect
    )
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(columnLookup);
        ArgumentNullException.ThrowIfNull(dialect);

        var indexColumns = rows
            .GroupAsDictionary(static row => new
            {
                row.IndexName,
                row.IsUnique,
                row.IsDisabled,
                row.IsFiltered,
                row.FilterDefinition,
                row.IndexType,
                row.FillFactor,
            })
            .ToList();
        if (indexColumns.Empty())
            return [];

        var result = new List<IDatabaseIndex>(indexColumns.Count);
        foreach (var indexInfo in indexColumns)
        {
            var isUnique = indexInfo.Key.IsUnique;
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);
            var isEnabled = !indexInfo.Key.IsDisabled;

            var indexCols = indexInfo.Value
                .Where(static row => !row.IsIncludedColumn)
                .OrderBy(static row => row.KeyOrdinal)
                .ThenBy(static row => row.IndexColumnId)
                .Select(row =>
                {
                    columnLookup.TryGetValue(row.ColumnName, out var column);
                    return new { row.IsDescending, Column = column };
                })
                .Where(static row => row.Column != null)
                .Select(row =>
                {
                    var order = row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                    var column = row.Column!;
                    var expression = dialect.QuoteName(column.Name);
                    return new DatabaseIndexColumn(expression, column, order);
                })
                .ToList();

            var includedCols = ResolveColumns(
                indexInfo.Value
                    .Where(static row => row.IsIncludedColumn)
                    .OrderBy(static row => row.KeyOrdinal)
                    .ThenBy(static row => row.ColumnName, StringComparer.Ordinal) // matches SSMS behaviour
                    .Select(static row => (Identifier)row.ColumnName),
                columnLookup
            ).ToList();

            var filterDefinition = indexInfo.Key.IsFiltered && !indexInfo.Key.FilterDefinition.IsNullOrWhiteSpace()
                ? Option<string>.Some(indexInfo.Key.FilterDefinition)
                : Option<string>.None;

            var indexType = IndexTypeMapping.TryGetValue(indexInfo.Key.IndexType, out var mappedIndexType)
                ? mappedIndexType
                : IndexType.Unknown;

            // a fill factor of zero means that the server default is used, i.e. no fill factor was set
            var fillFactor = indexInfo.Key.FillFactor > 0
                ? Option<int>.Some(indexInfo.Key.FillFactor)
                : Option<int>.None;

            var index = new DatabaseIndex(
                indexName,
                isUnique,
                indexCols,
                includedCols,
                isEnabled,
                filterDefinition,
                indexType,
                fillFactor,
                true, // SQL Server has no notion of an invalid index, only a disabled one
                true // nor of an invisible index
            );
            result.Add(index);
        }

        return result;
    }

    public static IReadOnlyCollection<IDatabaseTrigger> MapTriggers(IEnumerable<GetTableTriggers.Result> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var triggerRows = rows.ToList();
        if (triggerRows.Empty())
            return [];

        var result = new List<IDatabaseTrigger>(triggerRows.Count);
        foreach (var trig in triggerRows)
        {
            var triggerName = Identifier.CreateQualifiedIdentifier(trig.TriggerName);
            var queryTiming = trig.IsInsteadOfTrigger ? TriggerQueryTiming.InsteadOf : TriggerQueryTiming.After;
            var definition = trig.Definition;
            var isEnabled = !trig.IsDisabled;

            var events = TriggerEvent.None;
            if (trig.IsInsertTrigger)
                events |= TriggerEvent.Insert;
            if (trig.IsUpdateTrigger)
                events |= TriggerEvent.Update;
            if (trig.IsDeleteTrigger)
                events |= TriggerEvent.Delete;
            if (trig.IsOtherTrigger)
                events |= TriggerEvent.Other;

            // SQL Server DML triggers always fire once per statement; there is no FOR EACH ROW form.
            // Neither is there a WHEN clause or an UPDATE OF column list -- a trigger body tests
            // update(column) at runtime instead, which the catalog does not record.
            var trigger = new DatabaseTrigger(
                triggerName,
                definition,
                queryTiming,
                events,
                isEnabled,
                TriggerGranularity.Statement,
                Option<string>.None,
                []
            );
            result.Add(trigger);
        }

        return result;
    }

    // Resolves a sequence of column names against a lookup, preserving order and silently skipping any
    // name that has no corresponding column.
    private static IEnumerable<IDatabaseColumn> ResolveColumns(IEnumerable<Identifier> columnNames, IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup)
    {
        foreach (var name in columnNames)
        {
            if (columnLookup.TryGetValue(name, out var column))
                yield return column;
        }
    }

    // sys.indexes.type values, see https://learn.microsoft.com/en-us/sql/relational-databases/system-catalog-views/sys-indexes-transact-sql
    private static readonly IReadOnlyDictionary<int, IndexType> IndexTypeMapping = new Dictionary<int, IndexType>
    {
        [1] = IndexType.Clustered,
        [2] = IndexType.BTree,
        [3] = IndexType.Xml,
        [4] = IndexType.Spatial,
        [5] = IndexType.ColumnStore,
        [6] = IndexType.ColumnStore,
        [7] = IndexType.Hash,
    };
}
