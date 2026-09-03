using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Oracle.Queries;

namespace SJP.Schematic.Oracle;

/// <summary>
/// Maps the raw index and trigger rows returned by the Oracle catalog onto the core model. Indexes
/// and triggers attach to tables, views and materialized views in exactly the same shape, so the
/// table and view providers share this mapping rather than each carrying its own copy.
/// </summary>
internal static class OracleCatalogMapper
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
            .GroupAsDictionary(static row => new { row.IndexName, row.Uniqueness, row.IndexType, row.Status, row.Visibility })
            .ToList();
        if (indexColumns.Empty())
            return [];

        var result = new List<IDatabaseIndex>(indexColumns.Count);
        foreach (var indexInfo in indexColumns)
        {
            var isUnique = string.Equals(indexInfo.Key.Uniqueness, Unique, StringComparison.Ordinal);
            var indexName = Identifier.CreateQualifiedIdentifier(indexInfo.Key.IndexName);

            var indexCols = indexInfo.Value
                .Where(static row => row.ColumnName != null)
                .OrderBy(static row => row.ColumnPosition)
                .Select(static row => new { row.IsDescending, Column = row.ColumnName! })
                .Select(row =>
                {
                    var order = string.Equals(row.IsDescending, Y, StringComparison.Ordinal) ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;
                    var indexColumns = columnLookup.TryGetValue(row.Column, out var indexColumn)
                        ? [indexColumn]
                        : Array.Empty<IDatabaseColumn>();
                    var expression = dialect.QuoteName(row.Column);
                    return new DatabaseIndexColumn(expression, indexColumns, order);
                })
                .ToList();

            var indexType = indexInfo.Key.IndexType != null && IndexTypeMapping.TryGetValue(indexInfo.Key.IndexType, out var mappedIndexType)
                ? mappedIndexType
                : IndexType.Unknown;
            var isValid = !string.Equals(indexInfo.Key.Status, Unusable, StringComparison.Ordinal);
            var isVisible = !string.Equals(indexInfo.Key.Visibility, Invisible, StringComparison.Ordinal);

            var index = new OracleDatabaseIndex(indexName, isUnique, indexCols, indexType, isValid, isVisible);
            result.Add(index);
        }

        return result;
    }

    public static IReadOnlyCollection<IDatabaseTrigger> MapTriggers(IEnumerable<GetTableTriggers.Result> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var triggers = rows.ToList();
        if (triggers.Empty())
            return [];

        var result = new List<IDatabaseTrigger>(triggers.Count);
        foreach (var triggerRow in triggers)
        {
            var triggerName = Identifier.CreateQualifiedIdentifier(triggerRow.TriggerSchema, triggerRow.TriggerName);
            var queryTiming = triggerRow.TriggerType != null && TimingMapping.TryGetValue(triggerRow.TriggerType, out var timing)
                ? timing
                : TriggerQueryTiming.After;
            var granularity = triggerRow.TriggerType != null && GranularityMapping.TryGetValue(triggerRow.TriggerType, out var rowOrStatement)
                ? rowOrStatement
                : TriggerGranularity.Unknown;
            var definition = triggerRow.Definition ?? string.Empty;
            var isEnabled = string.Equals(triggerRow.EnabledStatus, Enabled, StringComparison.Ordinal);

            var events = TriggerEvent.None;
            var triggerEventPieces = triggerRow.TriggerEvent != null
                ? triggerRow.TriggerEvent.Split([" OR "], StringSplitOptions.RemoveEmptyEntries)
                : [];

            foreach (var triggerEventPiece in triggerEventPieces)
            {
                if (string.Equals(triggerEventPiece, Insert, StringComparison.Ordinal))
                    events |= TriggerEvent.Insert;
                else if (string.Equals(triggerEventPiece, Update, StringComparison.Ordinal))
                    events |= TriggerEvent.Update;
                else if (string.Equals(triggerEventPiece, Delete, StringComparison.Ordinal))
                    events |= TriggerEvent.Delete;
                else
                    events |= TriggerEvent.Other;
            }

            var condition = !triggerRow.Condition.IsNullOrWhiteSpace()
                ? Option<string>.Some(triggerRow.Condition)
                : Option<string>.None;
            var updateColumns = !triggerRow.UpdateColumns.IsNullOrWhiteSpace()
                ? triggerRow.UpdateColumns
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(static c => Identifier.CreateQualifiedIdentifier(c))
                    .ToList()
                : [];

            var trigger = new DatabaseTrigger(
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

    /// <summary>
    /// A mapping from the trigger query timings as described in Oracle, to a <see cref="TriggerQueryTiming"/> instance.
    /// </summary>
    public static IReadOnlyDictionary<string, TriggerQueryTiming> TimingMapping { get; } = new Dictionary<string, TriggerQueryTiming>(StringComparer.OrdinalIgnoreCase)
    {
        ["BEFORE STATEMENT"] = TriggerQueryTiming.Before,
        ["BEFORE EACH ROW"] = TriggerQueryTiming.Before,
        ["AFTER STATEMENT"] = TriggerQueryTiming.After,
        ["AFTER EACH ROW"] = TriggerQueryTiming.After,
        ["INSTEAD OF"] = TriggerQueryTiming.InsteadOf,
        ["COMPOUND"] = TriggerQueryTiming.Compound,
    };

    /// <summary>
    /// A mapping from the trigger types as described in Oracle, to a <see cref="TriggerGranularity"/> instance.
    /// A compound trigger has sections at both granularities, so it is reported as
    /// <see cref="TriggerGranularity.Unknown"/> rather than picking one of them.
    /// </summary>
    public static IReadOnlyDictionary<string, TriggerGranularity> GranularityMapping { get; } = new Dictionary<string, TriggerGranularity>(StringComparer.OrdinalIgnoreCase)
    {
        ["BEFORE STATEMENT"] = TriggerGranularity.Statement,
        ["BEFORE EACH ROW"] = TriggerGranularity.Row,
        ["AFTER STATEMENT"] = TriggerGranularity.Statement,
        ["AFTER EACH ROW"] = TriggerGranularity.Row,
        ["INSTEAD OF"] = TriggerGranularity.Row,
        ["COMPOUND"] = TriggerGranularity.Unknown,
    };

    private const string Delete = "DELETE";
    private const string Enabled = "ENABLED";
    private const string Insert = "INSERT";
    private const string Invisible = "INVISIBLE";
    private const string Unique = "UNIQUE";
    private const string Unusable = "UNUSABLE";
    private const string Update = "UPDATE";
    private const string Y = "Y";

    // ALL_INDEXES.INDEX_TYPE values, see the Oracle reference for ALL_INDEXES.
    private static readonly IReadOnlyDictionary<string, IndexType> IndexTypeMapping = new Dictionary<string, IndexType>(StringComparer.Ordinal)
    {
        ["NORMAL"] = IndexType.BTree,
        ["NORMAL/REV"] = IndexType.BTree,
        ["FUNCTION-BASED NORMAL"] = IndexType.BTree,
        ["FUNCTION-BASED NORMAL/REV"] = IndexType.BTree,
        ["BITMAP"] = IndexType.Bitmap,
        ["FUNCTION-BASED BITMAP"] = IndexType.Bitmap,
        ["IOT - TOP"] = IndexType.Clustered,
        ["DOMAIN"] = IndexType.Other,
        ["CLUSTER"] = IndexType.Other,
        ["LOB"] = IndexType.Other,
    };
}
