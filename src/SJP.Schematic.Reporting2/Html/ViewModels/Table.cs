using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The per-table detail payload (<c>data/tables/&lt;safeKey&gt;.json</c>): columns, keys,
/// constraints, indexes, triggers, and diagram references for one table.
/// </summary>
public sealed class Table : ITemplateParameter
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
        ulong rowCount
    )
    {
        ArgumentNullException.ThrowIfNull(tableName);

        Name = tableName.ToVisibleName();
        TableUrl = UrlRouter.GetTableUrl(tableName);
        RowCount = rowCount;

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
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Table;

    public string Name { get; }

    public string TableUrl { get; }

    public ulong RowCount { get; }

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
            IEnumerable<ParentKey> parentKeys
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
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public abstract class TableConstraint
    {
        protected TableConstraint(string constraintName)
        {
            ConstraintName = constraintName;
        }

        public string ConstraintName { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class PrimaryKeyConstraint : TableConstraint
    {
        public PrimaryKeyConstraint(string constraintName, IEnumerable<string> columns)
            : base(constraintName)
        {
            if (columns.NullOrEmpty())
                throw new ArgumentNullException(nameof(columns));

            ColumnNames = columns.Join(", ");
        }

        public string ColumnNames { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class UniqueKey : TableConstraint
    {
        public UniqueKey(string constraintName, IEnumerable<string> columns)
            : base(constraintName)
        {
            if (columns.NullOrEmpty())
                throw new ArgumentNullException(nameof(columns));

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
            ReferentialAction updateAction
        ) : base(constraintName)
        {
            if (columnNames.NullOrEmpty())
                throw new ArgumentNullException(nameof(columnNames));
            ArgumentNullException.ThrowIfNull(parentTableName);
            if (parentColumnNames.NullOrEmpty())
                throw new ArgumentNullException(nameof(parentColumnNames));
            if (!deleteAction.IsValid())
                throw new ArgumentException($"The {nameof(ReferentialAction)} provided must be a valid enum.", nameof(deleteAction));
            if (!updateAction.IsValid())
                throw new ArgumentException($"The {nameof(ReferentialAction)} provided must be a valid enum.", nameof(updateAction));

            ChildColumnNames = columnNames.Join(", ");
            ParentConstraintName = parentConstraintName;
            ParentTableName = parentTableName.ToVisibleName();
            ParentTableUrl = UrlRouter.GetTableUrl(parentTableName);
            ParentColumnNames = parentColumnNames.Join(", ");

            DeleteActionDescription = _actionDescription[deleteAction];
            UpdateActionDescription = _actionDescription[updateAction];
        }

        public string ParentConstraintName { get; }

        public string ChildColumnNames { get; }

        public string ParentTableName { get; }

        public string ParentTableUrl { get; }

        public string ParentColumnNames { get; }

        public string DeleteActionDescription { get; }

        public string UpdateActionDescription { get; }

        private static readonly IReadOnlyDictionary<ReferentialAction, string> _actionDescription = new Dictionary<ReferentialAction, string>
        {
            [ReferentialAction.NoAction] = "NO ACTION",
            [ReferentialAction.Restrict] = "RESTRICT",
            [ReferentialAction.Cascade] = "CASCADE",
            [ReferentialAction.SetDefault] = "SET DEFAULT",
            [ReferentialAction.SetNull] = "SET NULL",
        };
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class CheckConstraint : TableConstraint
    {
        public CheckConstraint(string constraintName, string definition)
            : base(constraintName)
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
            IEnumerable<string> includedColumnNames
        )
        {
            Name = indexName ?? string.Empty;
            IsUnique = isUnique;

            ColumnsText = columnNames.Zip(
                columnSorts.Select(SortToString),
                static (c, s) => c + " " + s
            ).Join(", ");
            IncludedColumnsText = includedColumnNames.Join(", ");
        }

        public string Name { get; }

        public bool IsUnique { get; }

        public string ColumnsText { get; }

        public string IncludedColumnsText { get; }

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
            TriggerEvent triggerEvent
        )
        {
            ArgumentNullException.ThrowIfNull(triggerName);
            ArgumentException.ThrowIfNullOrWhiteSpace(definition);

            TriggerName = triggerName.LocalName;
            Definition = definition;

            var queryFlags = queryTiming.GetFlags()
                .Select(static qt => TimingDescriptions[qt])
                .Order(StringComparer.Ordinal)
                .ToList();
            var eventFlags = triggerEvent.GetFlags()
                .Select(static te => EventDescriptions[te])
                .Order(StringComparer.Ordinal)
                .ToList();

            QueryTiming = queryFlags.Join(", ");
            Events = eventFlags.Join(", ");
        }

        public string TriggerName { get; }

        public string Definition { get; }

        public string QueryTiming { get; }

        public string Events { get; }

        private static readonly IReadOnlyDictionary<TriggerQueryTiming, string> TimingDescriptions = new Dictionary<TriggerQueryTiming, string>
        {
            [TriggerQueryTiming.After] = "AFTER",
            [TriggerQueryTiming.Before] = "BEFORE",
            [TriggerQueryTiming.InsteadOf] = "INSTEAD OF",
        };

        private static readonly IReadOnlyDictionary<TriggerEvent, string> EventDescriptions = new Dictionary<TriggerEvent, string>
        {
            [TriggerEvent.Delete] = "DELETE",
            [TriggerEvent.Insert] = "INSERT",
            [TriggerEvent.Update] = "UPDATE",
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
    /// A reference to a pre-rendered diagram SVG written under <c>data/diagrams/</c>.
    /// </summary>
    public sealed class Diagram
    {
        public Diagram(Identifier tableName, string diagramName, string dotDefinition, bool isActive)
        {
            ArgumentNullException.ThrowIfNull(tableName);
            ArgumentException.ThrowIfNullOrWhiteSpace(diagramName);
            ArgumentException.ThrowIfNullOrWhiteSpace(dotDefinition);

            Name = diagramName;
            Dot = dotDefinition;
            ContainerId = tableName.ToSafeKey() + "-" + Name.ToLowerInvariant().Replace(' ', '-') + "-chart";
            IsActive = isActive;
            SvgFile = "data/diagrams/" + ContainerId + ".svg";
        }

        public string Name { get; }

        public string ContainerId { get; }

        public bool IsActive { get; }

        public string SvgFile { get; }

        // The DOT source is consumed by the renderer to produce the SVG file; it is not part
        // of the JSON payload.
        [JsonIgnore]
        public string Dot { get; }
    }
}
