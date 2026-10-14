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
                .Select(row => MapIndexColumn(row, columnLookup, dialect))
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

    private static OracleDatabaseIndexColumn MapIndexColumn(
        GetTableIndexes.Result row,
        IReadOnlyDictionary<Identifier, IDatabaseColumn> columnLookup,
        IDatabaseDialect dialect
    )
    {
        var order = string.Equals(row.Descend, Desc, StringComparison.Ordinal) ? IndexColumnOrder.Descending : IndexColumnOrder.Ascending;

        // A descending or function-based index column is stored over a system-generated hidden column,
        // which is filtered out of the table/view's own columns. Its expression is the quoted column
        // name for a descending column, so that column is resolved instead. Any other expression is
        // reported as is, without dependent columns.
        var columnName = row.ColumnExpression.IsNullOrWhiteSpace()
            ? row.ColumnName!
            : TryGetQuotedName(row.ColumnExpression);
        if (columnName == null)
            return new OracleDatabaseIndexColumn(row.ColumnExpression!, order);

        var expression = dialect.QuoteName(columnName);
        return columnLookup.TryGetValue(columnName, out var indexColumn)
            ? new OracleDatabaseIndexColumn(expression, indexColumn, order)
            : new OracleDatabaseIndexColumn(expression, order);
    }

    // Oracle identifiers cannot contain a double quote, so a quoted name is exactly one pair of quotes.
    private static string? TryGetQuotedName(string expression)
    {
        var trimmed = expression.AsSpan().Trim();
        if (trimmed.Length < 3 || trimmed[0] != '"' || trimmed[^1] != '"')
            return null;

        var name = trimmed[1..^1];
        return name.Contains('"') ? null : name.ToString();
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
    /// Maps a <c>DELETE_RULE</c> value from the Oracle catalog onto a <see cref="ReferentialAction"/>.
    /// </summary>
    /// <param name="deleteRule">A <c>DELETE_RULE</c> value, which is <see langword="null" /> for any
    /// constraint that is not a foreign key, and for a foreign key whose parent rows cannot be deleted
    /// at all -- Oracle's <c>NO ACTION</c> behaviour.</param>
    /// <returns>The mapped referential action, or <see cref="ReferentialAction.NoAction"/> when the value
    /// is <see langword="null" /> or not one Oracle is known to report.</returns>
    public static ReferentialAction GetReferentialAction(string? deleteRule)
    {
        return deleteRule != null && ReferentialActionMapping.TryGetValue(deleteRule, out var action)
            ? action
            : ReferentialAction.NoAction;
    }

    /// <summary>
    /// Builds the numeric precision of a column, attribute or argument from the catalog's
    /// <c>DATA_PRECISION</c> and <c>DATA_SCALE</c> values (or their equivalents).
    /// </summary>
    /// <param name="typeName">The catalog's type name for the value, e.g. <c>NUMBER</c> or <c>TIMESTAMP(6)</c>.</param>
    /// <param name="precision">The catalog's precision, with <see langword="null" /> read as zero.</param>
    /// <param name="scale">The catalog's scale, with <see langword="null" /> read as zero.</param>
    /// <returns>A numeric precision, or <see cref="Option{A}.None"/> when the type does not have one.</returns>
    /// <remarks>
    /// The catalog reports a scale without a precision for <c>NUMBER(*, s)</c>, which has Oracle's
    /// maximum precision of 38, and also for a timestamp or a day-to-second interval with a zero
    /// leading precision, where the scale is the precision of its fractional seconds. That is read
    /// from the type name instead, so no numeric precision is reported. A negative scale, as in
    /// <c>NUMBER(5, -2)</c>, rounds to the left of the decimal point and cannot be represented, so
    /// it is reported as a scale of zero.
    /// </remarks>
    public static Option<INumericPrecision> GetNumericPrecision(string? typeName, int precision, int scale)
    {
        var nonNegativeScale = Math.Max(scale, 0);
        if (precision > 0)
            return Option<INumericPrecision>.Some(new NumericPrecision(precision, nonNegativeScale));

        return nonNegativeScale > 0 && string.Equals(typeName, Number, StringComparison.Ordinal)
            ? Option<INumericPrecision>.Some(new NumericPrecision(MaxNumberPrecision, nonNegativeScale))
            : Option<INumericPrecision>.None;
    }

    /// <summary>
    /// A mapping from the referential actions as described in Oracle, to a <see cref="ReferentialAction"/> instance.
    /// </summary>
    public static IReadOnlyDictionary<string, ReferentialAction> ReferentialActionMapping { get; } = new Dictionary<string, ReferentialAction>(StringComparer.OrdinalIgnoreCase)
    {
        ["NO ACTION"] = ReferentialAction.NoAction,
        ["RESTRICT"] = ReferentialAction.Restrict,
        ["CASCADE"] = ReferentialAction.Cascade,
        ["SET NULL"] = ReferentialAction.SetNull,
        ["SET DEFAULT"] = ReferentialAction.SetDefault,
    };

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
    private const string Desc = "DESC";
    private const string Enabled = "ENABLED";
    private const string Insert = "INSERT";
    private const string Invisible = "INVISIBLE";
    private const int MaxNumberPrecision = 38;
    private const string Number = "NUMBER";
    private const string Unique = "UNIQUE";
    private const string Unusable = "UNUSABLE";
    private const string Update = "UPDATE";

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
