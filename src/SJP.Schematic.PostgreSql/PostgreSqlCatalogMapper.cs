using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Queries;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// Maps the raw index and trigger rows returned by the PostgreSQL catalog onto the core model.
/// Indexes and triggers attach to tables, views and materialized views in exactly the same shape,
/// so the table and view providers share this mapping rather than each carrying its own copy.
/// </summary>
internal static class PostgreSqlCatalogMapper
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
                row.IsPrimary,
                row.FilterDefinition,
                row.KeyColumnCount,
                row.IndexMethod,
                row.IsValid,
            })
            .ToList();
        if (indexColumns.Empty())
            return [];

        var result = new List<IDatabaseIndex>(indexColumns.Count);
        foreach (var indexInfo in indexColumns)
        {
            var isUnique = indexInfo.Key.IsUnique;
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

            var filterDefinition = !indexInfo.Key.FilterDefinition.IsNullOrWhiteSpace()
                ? Option<string>.Some(indexInfo.Key.FilterDefinition)
                : Option<string>.None;

            // sorted once and reused for both the key and included columns below, instead of sorting
            // the same rows twice. NOTE: the two branches deliberately keep their existing, slightly
            // different filter ordering relative to Take/Skip (key columns filter nulls before Take,
            // included columns filter nulls after Skip) -- preserved as-is, not a perf-motivated change.
            var sortedRows = indexInfo.Value.OrderBy(static row => row.IndexColumnId).ToList();

            var indexCols = sortedRows
                .Where(static row => row.IndexColumnExpression != null)
                .Select(row => new
                {
                    row.IsDescending,
                    row.IsNullsFirst,
                    row.IndexColumnCollation,
                    Expression = row.IndexColumnExpression,
                    Column = row.IndexColumnExpression != null && columnLookup.TryGetValue(row.IndexColumnExpression, out var indexColumn)
                        ? indexColumn
                        : null,
                })
                .Select(row =>
                {
                    var order = row.IsDescending ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                    var nullOrder = row.IsNullsFirst ? IndexColumnNullOrder.NullsFirst : IndexColumnNullOrder.NullsLast;
                    var collation = !row.IndexColumnCollation.IsNullOrWhiteSpace()
                        ? Option<Identifier>.Some(Identifier.CreateQualifiedIdentifier(row.IndexColumnCollation))
                        : Option<Identifier>.None;
                    var expression = row.Column != null
                        ? dialect.QuoteName(row.Column.Name)
                        : row.Expression!;
                    return row.Column != null
                        ? new PostgreSqlDatabaseIndexColumn(expression, row.Column, order, nullOrder, collation)
                        : new PostgreSqlDatabaseIndexColumn(expression, order, nullOrder, collation);
                })
                .Take(indexInfo.Key.KeyColumnCount)
                .ToList();
            var includedCols = ResolveColumns(
                sortedRows
                    .Skip(indexInfo.Key.KeyColumnCount)
                    .Where(static row => row.IndexColumnExpression != null)
                    .Select(static row => (Identifier)row.IndexColumnExpression!),
                columnLookup
            ).ToList();

            var indexType = IndexTypeMapping.TryGetValue(indexInfo.Key.IndexMethod, out var mappedIndexType)
                ? mappedIndexType
                : IndexType.Other;

            var index = new PostgreSqlDatabaseIndex(indexName, isUnique, indexCols, includedCols, filterDefinition, indexType, indexInfo.Key.IsValid);
            result.Add(index);
        }

        return result;
    }

    public static IReadOnlyCollection<IDatabaseTrigger> MapTriggers(IEnumerable<GetTableTriggers.Result> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var triggers = rows.GroupAsDictionary(static row => new
        {
            row.TriggerName,
            row.Definition,
            row.Timing,
            row.Granularity,
            row.Condition,
            row.EnabledFlag,
        }).ToList();
        if (triggers.Empty())
            return [];

        var result = new List<IDatabaseTrigger>(triggers.Count);
        foreach (var trig in triggers)
        {
            var triggerName = Identifier.CreateQualifiedIdentifier(trig.Key.TriggerName);
            var queryTiming = ParseQueryTiming(trig.Key.Timing);
            var definition = trig.Key.Definition;

            var events = TriggerEvent.None;
            foreach (var triggerEvent in trig.Value.Select(t => t.TriggerEvent))
            {
                if (string.Equals(triggerEvent, Insert, StringComparison.Ordinal))
                    events |= TriggerEvent.Insert;
                else if (string.Equals(triggerEvent, Update, StringComparison.Ordinal))
                    events |= TriggerEvent.Update;
                else if (string.Equals(triggerEvent, Delete, StringComparison.Ordinal))
                    events |= TriggerEvent.Delete;
                else if (string.Equals(triggerEvent, Truncate, StringComparison.Ordinal))
                    events |= TriggerEvent.Truncate;
                else
                    events |= TriggerEvent.Other;
            }

            var granularity = string.Equals(trig.Key.Granularity, Row, StringComparison.Ordinal)
                ? TriggerGranularity.Row
                : TriggerGranularity.Statement;
            var condition = !trig.Key.Condition.IsNullOrWhiteSpace()
                ? Option<string>.Some(trig.Key.Condition)
                : Option<string>.None;
            // tgattr is per-trigger, so any row of the group carries the same UPDATE OF column list.
            var updateColumns = trig.Value[0].UpdateColumns?
                .Select(static c => Identifier.CreateQualifiedIdentifier(c))
                .ToList() ?? [];

            var isEnabled = !string.Equals(trig.Key.EnabledFlag, DisabledFlag, StringComparison.Ordinal);
            var trigger = new PostgreSqlDatabaseTrigger(
                triggerName,
                definition,
                queryTiming,
                events,
                isEnabled,
                granularity,
                condition,
                updateColumns
            );
            result.Add(trigger);
        }

        return result;
    }

    // 'INSTEAD OF' does not parse as a TriggerQueryTiming member on its own, so it is matched first.
    private static TriggerQueryTiming ParseQueryTiming(string timing)
    {
        if (string.Equals(timing, InsteadOf, StringComparison.OrdinalIgnoreCase))
            return TriggerQueryTiming.InsteadOf;

        return Enum.TryParse(timing, true, out TriggerQueryTiming parsedTiming)
            ? parsedTiming
            : TriggerQueryTiming.Before;
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

    private const string Delete = "DELETE";
    private const string DisabledFlag = "D";
    private const string Insert = "INSERT";
    private const string InsteadOf = "INSTEAD OF";
    private const string Row = "ROW";
    private const string Truncate = "TRUNCATE";
    private const string Update = "UPDATE";

    // pg_am.amname values for the access methods shipped with PostgreSQL. Anything else, e.g. an
    // access method provided by an extension, is reported as IndexType.Other.
    private static readonly IReadOnlyDictionary<string, IndexType> IndexTypeMapping = new Dictionary<string, IndexType>(StringComparer.OrdinalIgnoreCase)
    {
        ["btree"] = IndexType.BTree,
        ["hash"] = IndexType.Hash,
        ["gin"] = IndexType.Gin,
        ["gist"] = IndexType.Gist,
        ["brin"] = IndexType.Brin,
    };
}
