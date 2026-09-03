using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The per-table detail payload (<c>data/tables/&lt;safeKey&gt;.json</c>): columns, keys,
/// constraints, indexes, triggers, and diagram references for one table.
/// </summary>
public sealed class Table
{
    public Table(
        Identifier tableName,
        IEnumerable<Column> columns,
        Option<PrimaryKeyConstraint> primaryKey,
        IEnumerable<UniqueKey> uniqueKeys,
        IEnumerable<ForeignKey> foreignKeys,
        IEnumerable<CheckConstraint> checks,
        IEnumerable<Index> indexes,
        IEnumerable<Trigger> triggers,
        IEnumerable<Diagram> diagrams,
        TableKind kind,
        Option<Partitioning> partitioning,
        Option<SystemVersioning> systemVersioning,
        bool isLogged,
        Option<Identifier> collation
    )
    {
        ArgumentNullException.ThrowIfNull(tableName);

        Name = tableName.ToVisibleName();
        TableUrl = UrlRouter.GetTableUrl(tableName);

        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        ColumnsCount = columns.UCount();

        PrimaryKey = primaryKey.MatchUnsafe(static pk => pk, static () => (PrimaryKeyConstraint?)null);
        PrimaryKeyExists = primaryKey.IsSome;

        UniqueKeys = uniqueKeys ?? throw new ArgumentNullException(nameof(uniqueKeys));
        UniqueKeysCount = uniqueKeys.UCount();

        ForeignKeys = foreignKeys ?? throw new ArgumentNullException(nameof(foreignKeys));
        ForeignKeysCount = foreignKeys.UCount();

        CheckConstraints = checks ?? throw new ArgumentNullException(nameof(checks));
        CheckConstraintsCount = checks.UCount();

        Indexes = indexes ?? throw new ArgumentNullException(nameof(indexes));
        IndexesCount = indexes.UCount();

        Triggers = triggers ?? throw new ArgumentNullException(nameof(triggers));
        TriggersCount = triggers.UCount();

        Diagrams = diagrams ?? throw new ArgumentNullException(nameof(diagrams));

        Kind = TableKindNames.GetName(kind);
        TablePartitioning = partitioning.MatchUnsafe(static p => p, static () => (Partitioning?)null);
        TableSystemVersioning = systemVersioning.MatchUnsafe(static sv => sv, static () => (SystemVersioning?)null);
        IsLogged = isLogged;
        Collation = collation.Match(static name => name.ToVisibleName(), static () => string.Empty);
    }

    public string Name { get; }

    public string TableUrl { get; }

    public IEnumerable<Column> Columns { get; }

    public uint ColumnsCount { get; }

    public PrimaryKeyConstraint? PrimaryKey { get; }

    public bool PrimaryKeyExists { get; }

    public IEnumerable<UniqueKey> UniqueKeys { get; }

    public uint UniqueKeysCount { get; }

    public IEnumerable<ForeignKey> ForeignKeys { get; }

    public uint ForeignKeysCount { get; }

    public IEnumerable<CheckConstraint> CheckConstraints { get; }

    public uint CheckConstraintsCount { get; }

    public IEnumerable<Index> Indexes { get; }

    public uint IndexesCount { get; }

    public IEnumerable<Trigger> Triggers { get; }

    public uint TriggersCount { get; }

    public IEnumerable<Diagram> Diagrams { get; }

    /// <summary>
    /// What the table is, where that differs from an ordinary persistent table. Empty for an
    /// ordinary table.
    /// </summary>
    public string Kind { get; }

    /// <summary>
    /// How the table's rows are distributed across partitions. Null when the table is not partitioned.
    /// </summary>
    public Partitioning? TablePartitioning { get; }

    /// <summary>
    /// Where the table's superseded rows are retained. Null when the table is not system-versioned.
    /// </summary>
    public SystemVersioning? TableSystemVersioning { get; }

    /// <summary>
    /// Whether writes to the table are written to the database's transaction log.
    /// </summary>
    public bool IsLogged { get; }

    /// <summary>
    /// The default collation applied to the table's character data. Empty when the database records
    /// none for the table as a whole.
    /// </summary>
    public string Collation { get; }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Partitioning
    {
        public Partitioning(string strategy, IEnumerable<string> columnNames, IEnumerable<LinkedTable> partitions)
        {
            Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            ColumnNames = columnNames ?? throw new ArgumentNullException(nameof(columnNames));
            Partitions = partitions ?? throw new ArgumentNullException(nameof(partitions));
            PartitionsCount = partitions.UCount();
        }

        public string Strategy { get; }

        public IEnumerable<string> ColumnNames { get; }

        public IEnumerable<LinkedTable> Partitions { get; }

        public uint PartitionsCount { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class SystemVersioning
    {
        public SystemVersioning(LinkedTable historyTable, string periodStartColumn, string periodEndColumn)
        {
            HistoryTable = historyTable ?? throw new ArgumentNullException(nameof(historyTable));
            PeriodStartColumn = periodStartColumn ?? throw new ArgumentNullException(nameof(periodStartColumn));
            PeriodEndColumn = periodEndColumn ?? throw new ArgumentNullException(nameof(periodEndColumn));
        }

        public LinkedTable HistoryTable { get; }

        public string PeriodStartColumn { get; }

        public string PeriodEndColumn { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class LinkedTable
    {
        /// <param name="name">The name of the object being referred to.</param>
        /// <param name="isTable">Whether the report contains a page for the object. Partitions are named segments of a table rather than tables in their own right in every database but PostgreSQL, so they usually have no page to link to.</param>
        public LinkedTable(Identifier name, bool isTable)
        {
            ArgumentNullException.ThrowIfNull(name);

            Name = name.ToVisibleName();
            TableUrl = isTable ? UrlRouter.GetTableUrl(name) : string.Empty;
        }

        public string Name { get; }

        public string TableUrl { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Column
    {
        public Column(
            string columnName,
            int ordinal,
            bool isNullable,
            string typeDefinition,
            Option<string> defaultValue,
            bool isPrimaryKeyColumn,
            bool isUniqueKeyColumn,
            bool isForeignKeyColumn,
            IEnumerable<ChildKey> childKeys,
            IEnumerable<ParentKey> parentKeys,
            Option<IAutoIncrement> autoIncrement,
            bool isComputed,
            Option<string> computedDefinition,
            ComputedColumnStorage computedStorage
        )
        {
            ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            Ordinal = ordinal;
            IsNullable = isNullable;
            Type = typeDefinition ?? string.Empty;
            DefaultValue = defaultValue.Match(static def => def ?? string.Empty, static () => string.Empty);

            IsPrimaryKey = isPrimaryKeyColumn;
            IsUniqueKey = isUniqueKeyColumn;
            IsForeignKey = isForeignKeyColumn;

            ChildKeys = childKeys ?? throw new ArgumentNullException(nameof(childKeys));
            ChildKeysCount = childKeys.UCount();

            ParentKeys = parentKeys ?? throw new ArgumentNullException(nameof(parentKeys));
            ParentKeysCount = parentKeys.UCount();

            IsAutoIncrement = autoIncrement.IsSome;
            IdentityGeneration = autoIncrement.Match(static incr => IdentityGenerationNames.GetName(incr.Generation), static () => string.Empty);
            IdentitySequenceName = autoIncrement
                .Bind(static incr => incr.SequenceName)
                .Match(static name => name.ToVisibleName(), static () => string.Empty);

            IsComputed = isComputed;
            ComputedDefinition = computedDefinition.Match(static def => def ?? string.Empty, static () => string.Empty);
            ComputedStorage = ComputedColumnStorageNames.GetName(computedStorage);
        }

        public int Ordinal { get; }

        public string ColumnName { get; }

        public bool IsNullable { get; }

        public string Type { get; }

        public string DefaultValue { get; }

        public bool IsPrimaryKey { get; }

        public bool IsUniqueKey { get; }

        public bool IsForeignKey { get; }

        public IEnumerable<ParentKey> ParentKeys { get; }

        public uint ParentKeysCount { get; }

        public IEnumerable<ChildKey> ChildKeys { get; }

        public uint ChildKeysCount { get; }

        public bool IsAutoIncrement { get; }

        public string IdentityGeneration { get; }

        public string IdentitySequenceName { get; }

        public bool IsComputed { get; }

        public string ComputedDefinition { get; }

        public string ComputedStorage { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public abstract class TableConstraint
    {
        protected TableConstraint(string constraintName, bool isValidated, ConstraintDeferrability deferrability)
        {
            if (!deferrability.IsValid())
                throw new ArgumentException($"The {nameof(ConstraintDeferrability)} provided must be a valid enum.", nameof(deferrability));

            ConstraintName = constraintName;
            IsValidated = isValidated;
            DeferrabilityDescription = ConstraintStateNames.GetDeferrabilityName(deferrability);
        }

        public string ConstraintName { get; }

        public bool IsValidated { get; }

        public string DeferrabilityDescription { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class PrimaryKeyConstraint : TableConstraint
    {
        public PrimaryKeyConstraint(string constraintName, IEnumerable<string> columns, bool isValidated, ConstraintDeferrability deferrability)
            : base(constraintName, isValidated, deferrability)
        {
            ArgumentNullException.ThrowIfNull(columns);
            if (columns.Empty())
                throw new ArgumentException("A key must have at least one column.", nameof(columns));

            ColumnNames = columns.Join(", ");
        }

        public string ColumnNames { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class UniqueKey : TableConstraint
    {
        public UniqueKey(string constraintName, IEnumerable<string> columns, bool isValidated, ConstraintDeferrability deferrability)
            : base(constraintName, isValidated, deferrability)
        {
            ArgumentNullException.ThrowIfNull(columns);
            if (columns.Empty())
                throw new ArgumentException("A key must have at least one column.", nameof(columns));

            ColumnNames = columns.Join(", ");
        }

        public string ColumnNames { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class ForeignKey : TableConstraint
    {
        public ForeignKey(
            string constraintName,
            IEnumerable<string> columnNames,
            Identifier parentTableName,
            string parentConstraintName,
            IEnumerable<string> parentColumnNames,
            ReferentialAction deleteAction,
            ReferentialAction updateAction,
            bool isValidated,
            ConstraintDeferrability deferrability,
            ForeignKeyMatchType matchType
        ) : base(constraintName, isValidated, deferrability)
        {
            ArgumentNullException.ThrowIfNull(columnNames);
            if (columnNames.Empty())
                throw new ArgumentException("A foreign key must have at least one column.", nameof(columnNames));
            ArgumentNullException.ThrowIfNull(parentTableName);
            ArgumentNullException.ThrowIfNull(parentColumnNames);
            if (parentColumnNames.Empty())
                throw new ArgumentException("A foreign key must refer to at least one parent column.", nameof(parentColumnNames));
            if (!deleteAction.IsValid())
                throw new ArgumentException($"The {nameof(ReferentialAction)} provided must be a valid enum.", nameof(deleteAction));
            if (!updateAction.IsValid())
                throw new ArgumentException($"The {nameof(ReferentialAction)} provided must be a valid enum.", nameof(updateAction));
            if (!matchType.IsValid())
                throw new ArgumentException($"The {nameof(ForeignKeyMatchType)} provided must be a valid enum.", nameof(matchType));

            ChildColumnNames = columnNames.Join(", ");
            ParentConstraintName = parentConstraintName;
            ParentTableName = parentTableName.ToVisibleName();
            ParentTableUrl = UrlRouter.GetTableUrl(parentTableName);
            ParentColumnNames = parentColumnNames.Join(", ");

            DeleteActionDescription = GetActionDescription(deleteAction);
            UpdateActionDescription = GetActionDescription(updateAction);
            MatchTypeDescription = ConstraintStateNames.GetMatchTypeName(matchType);
        }

        public string ParentConstraintName { get; }

        public string ChildColumnNames { get; }

        public string ParentTableName { get; }

        public string ParentTableUrl { get; }

        public string ParentColumnNames { get; }

        public string DeleteActionDescription { get; }

        public string UpdateActionDescription { get; }

        public string MatchTypeDescription { get; }

        private static string GetActionDescription(ReferentialAction action) => action switch
        {
            ReferentialAction.NoAction => "NO ACTION",
            ReferentialAction.Restrict => "RESTRICT",
            ReferentialAction.Cascade => "CASCADE",
            ReferentialAction.SetDefault => "SET DEFAULT",
            ReferentialAction.SetNull => "SET NULL",
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class CheckConstraint : TableConstraint
    {
        public CheckConstraint(string constraintName, string definition, bool isValidated, ConstraintDeferrability deferrability)
            : base(constraintName, isValidated, deferrability)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(definition);

            Definition = definition;
        }

        public string Definition { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Index
    {
        public Index(
            string? indexName,
            bool isUnique,
            IEnumerable<string> columnNames,
            IEnumerable<IndexColumnOrder> columnSorts,
            IEnumerable<string> includedColumnNames,
            IndexType indexType,
            Option<string> filterDefinition,
            bool isEnabled,
            bool isValid,
            bool isVisible
        )
        {
            Name = indexName ?? string.Empty;
            IsUnique = isUnique;

            ColumnsText = columnNames.Zip(
                columnSorts.Select(SortToString),
                static (c, s) => c + " " + s
            ).Join(", ");
            IncludedColumnsText = includedColumnNames.Join(", ");

            IndexType = IndexTypeNames.GetName(indexType);
            FilterText = filterDefinition.Match(static filter => filter ?? string.Empty, static () => string.Empty);
            IsEnabled = isEnabled;
            IsValid = isValid;
            IsVisible = isVisible;
        }

        public string Name { get; }

        public bool IsUnique { get; }

        public string ColumnsText { get; }

        public string IncludedColumnsText { get; }

        public string IndexType { get; }

        public string FilterText { get; }

        public bool IsEnabled { get; }

        public bool IsValid { get; }

        public bool IsVisible { get; }

        private static string SortToString(IndexColumnOrder order)
        {
            return order == IndexColumnOrder.Ascending
                ? "ASC"
                : "DESC";
        }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Trigger
    {
        public Trigger(
            Identifier triggerName,
            string definition,
            TriggerQueryTiming queryTiming,
            TriggerEvent triggerEvent,
            TriggerGranularity granularity,
            Option<string> condition,
            IEnumerable<Identifier> updateColumns
        )
        {
            ArgumentNullException.ThrowIfNull(triggerName);
            ArgumentException.ThrowIfNullOrWhiteSpace(definition);
            ArgumentNullException.ThrowIfNull(updateColumns);

            TriggerName = triggerName.LocalName;
            Definition = definition;

            var eventFlags = triggerEvent.GetFlags()
                .Select(static te => GetEventDescription(te))
                .Order(StringComparer.Ordinal)
                .ToList();

            QueryTiming = GetTimingDescription(queryTiming);
            Events = eventFlags.Join(", ");
            Granularity = GetGranularityDescription(granularity);
            Condition = condition.MatchUnsafe(static c => c, static () => string.Empty) ?? string.Empty;
            UpdateColumns = updateColumns.Select(static c => c.LocalName).Join(", ");
        }

        public string TriggerName { get; }

        public string Definition { get; }

        public string QueryTiming { get; }

        public string Events { get; }

        /// <summary>How often the trigger fires. Empty when the database did not report a granularity.</summary>
        public string Granularity { get; }

        /// <summary>The trigger's <c>WHEN</c> clause. Empty when the trigger is unconditional.</summary>
        public string Condition { get; }

        /// <summary>The trigger's <c>UPDATE OF</c> column list. Empty when updates to any column fire it.</summary>
        public string UpdateColumns { get; }

        private static string GetTimingDescription(TriggerQueryTiming timing) => timing switch
        {
            TriggerQueryTiming.After => "AFTER",
            TriggerQueryTiming.Before => "BEFORE",
            TriggerQueryTiming.InsteadOf => "INSTEAD OF",
            TriggerQueryTiming.Compound => "COMPOUND",
            _ => throw new ArgumentOutOfRangeException(nameof(timing)),
        };

        private static string GetGranularityDescription(TriggerGranularity granularity) => granularity switch
        {
            TriggerGranularity.Row => "FOR EACH ROW",
            TriggerGranularity.Statement => "FOR EACH STATEMENT",
            TriggerGranularity.Unknown => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(granularity)),
        };

        private static string GetEventDescription(TriggerEvent triggerEvent) => triggerEvent switch
        {
            TriggerEvent.Delete => "DELETE",
            TriggerEvent.Insert => "INSERT",
            TriggerEvent.Update => "UPDATE",
            TriggerEvent.Truncate => "TRUNCATE",
            TriggerEvent.Other => "OTHER",
            _ => throw new ArgumentOutOfRangeException(nameof(triggerEvent)),
        };
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class ParentKey
    {
        public ParentKey(string constraintName, Identifier parentTableName, string parentColumnName, string qualifiedChildColumnName)
        {
            ArgumentNullException.ThrowIfNull(parentTableName);
            ArgumentException.ThrowIfNullOrWhiteSpace(parentColumnName);
            ArgumentException.ThrowIfNullOrWhiteSpace(qualifiedChildColumnName);

            ParentTableName = parentTableName.ToVisibleName();
            ParentTableUrl = UrlRouter.GetTableUrl(parentTableName);
            ParentColumnName = parentColumnName;

            var qualifiedParentColumnName = ParentTableName + "." + parentColumnName;
            var description = qualifiedChildColumnName + " references " + qualifiedParentColumnName;
            if (!constraintName.IsNullOrWhiteSpace())
                description += " via " + constraintName;
            ConstraintDescription = description;
        }

        public string ConstraintDescription { get; }

        public string ParentTableName { get; }

        public string ParentTableUrl { get; }

        public string ParentColumnName { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class ChildKey
    {
        public ChildKey(string constraintName, Identifier childTableName, string childColumnName, string qualifiedParentColumnName)
        {
            ArgumentNullException.ThrowIfNull(childTableName);
            ArgumentException.ThrowIfNullOrWhiteSpace(childColumnName);
            ArgumentException.ThrowIfNullOrWhiteSpace(qualifiedParentColumnName);

            ChildTableName = childTableName.ToVisibleName();
            ChildTableUrl = UrlRouter.GetTableUrl(childTableName);
            ChildColumnName = childColumnName;

            var qualifiedChildColumnName = ChildTableName + "." + ChildColumnName;
            var description = qualifiedChildColumnName + " references " + qualifiedParentColumnName;
            if (!constraintName.IsNullOrWhiteSpace())
                description += " via " + constraintName;
            ConstraintDescription = description;
        }

        public string ConstraintDescription { get; }

        public string ChildTableName { get; }

        public string ChildTableUrl { get; }

        public string ChildColumnName { get; }
    }

    /// <summary>
    /// A per-table relationship diagram (e.g. "One Degree" / "Two Degrees") as graph data laid out and
    /// drawn client-side. The two levels differ in which neighbouring tables they include, so each
    /// carries its own <see cref="RelationshipGraph"/>.
    /// </summary>
    public sealed class Diagram
    {
        public Diagram(Identifier tableName, string diagramName, RelationshipGraph graph, bool isActive)
        {
            ArgumentNullException.ThrowIfNull(tableName);
            ArgumentException.ThrowIfNullOrWhiteSpace(diagramName);

            Name = diagramName;
            Graph = graph ?? throw new ArgumentNullException(nameof(graph));
            ContainerId = tableName.ToSafeKey() + "-" + Name.ToLowerInvariant().Replace(' ', '-') + "-chart";
            IsActive = isActive;
        }

        public string Name { get; }

        public string ContainerId { get; }

        public bool IsActive { get; }

        public RelationshipGraph Graph { get; }
    }
}
