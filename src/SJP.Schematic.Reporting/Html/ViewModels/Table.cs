using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Internal. Not intended to be used outside of this assembly. Only required for templating.
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
        string rootPath,
        ulong rowCount
    )
    {
        if (tableName == null)
            throw new ArgumentNullException(nameof(tableName));

        Name = tableName.ToVisibleName();
        TableUrl = tableName.ToSafeKey();
        RowCount = rowCount;

        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        ColumnsCount = columns.UCount();
        ColumnsTableClass = ColumnsCount > 0 ? CssClasses.DataTableClass : string.Empty;

        PrimaryKey = primaryKey.MatchUnsafe(static pk => pk, static () => (PrimaryKeyConstraint?)null);
        PrimaryKeyExists = primaryKey.IsSome;
        PrimaryKeyTableClass = primaryKey.Match(static _ => CssClasses.DataTableClass, static () => string.Empty);

        UniqueKeys = uniqueKeys ?? throw new ArgumentNullException(nameof(uniqueKeys));
        UniqueKeysCount = uniqueKeys.UCount();
        UniqueKeysTableClass = UniqueKeysCount > 0 ? CssClasses.DataTableClass : string.Empty;

        ForeignKeys = foreignKeys ?? throw new ArgumentNullException(nameof(foreignKeys));
        ForeignKeysCount = foreignKeys.UCount();
        ForeignKeysTableClass = ForeignKeysCount > 0 ? CssClasses.DataTableClass : string.Empty;

        CheckConstraints = checks ?? throw new ArgumentNullException(nameof(checks));
        CheckConstraintsCount = checks.UCount();
        CheckConstraintsTableClass = CheckConstraintsCount > 0 ? CssClasses.DataTableClass : string.Empty;

        Indexes = indexes ?? throw new ArgumentNullException(nameof(checks));
        IndexesCount = indexes.UCount();
        IndexesTableClass = IndexesCount > 0 ? CssClasses.DataTableClass : string.Empty;

        Triggers = triggers ?? throw new ArgumentNullException(nameof(triggers));
        TriggersCount = triggers.UCount();
        TriggersTableClass = TriggersCount > 0 ? CssClasses.DataTableClass : string.Empty;

        Diagrams = diagrams ?? throw new ArgumentNullException(nameof(diagrams));

        RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
    }

    public ReportTemplate Template { get; } = ReportTemplate.Table;

    public string RootPath { get; }

    public string Name { get; }

    public string TableUrl { get; }

    public ulong RowCount { get; }

    public IEnumerable<Column> Columns { get; }

    public uint ColumnsCount { get; }

    public HtmlString ColumnsTableClass { get; }

    public PrimaryKeyConstraint? PrimaryKey { get; }

    public bool PrimaryKeyExists { get; }

    public HtmlString PrimaryKeyTableClass { get; }

    public IEnumerable<UniqueKey> UniqueKeys { get; }

    public uint UniqueKeysCount { get; }

    public HtmlString UniqueKeysTableClass { get; }

    public IEnumerable<ForeignKey> ForeignKeys { get; }

    public uint ForeignKeysCount { get; }

    public HtmlString ForeignKeysTableClass { get; }

    public IEnumerable<CheckConstraint> CheckConstraints { get; }

    public uint CheckConstraintsCount { get; }

    public HtmlString CheckConstraintsTableClass { get; }

    public IEnumerable<Index> Indexes { get; }

    public uint IndexesCount { get; }

    public HtmlString IndexesTableClass { get; }

    public IEnumerable<Trigger> Triggers { get; }

    public uint TriggersCount { get; }

    public HtmlString TriggersTableClass { get; }

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
            TitleNullable = isNullable ? "Nullable" : string.Empty;
            NullableText = isNullable ? "✓" : "✗";
            Type = typeDefinition ?? string.Empty;
            DefaultValue = defaultValue.Match(static def => def ?? string.Empty, static () => string.Empty);

            ColumnClass = BuildColumnClass(isPrimaryKeyColumn, isUniqueKeyColumn, isForeignKeyColumn);
            ColumnIcon = BuildColumnIcon(isPrimaryKeyColumn, isUniqueKeyColumn, isForeignKeyColumn);
            ColumnTitle = BuildColumnTitle(isPrimaryKeyColumn, isUniqueKeyColumn, isForeignKeyColumn);

            ChildKeys = childKeys ?? throw new ArgumentNullException(nameof(childKeys));
            ChildKeysCount = childKeys.UCount();

            ParentKeys = parentKeys ?? throw new ArgumentNullException(nameof(parentKeys));
            ParentKeysCount = parentKeys.UCount();
        }

        public int Ordinal { get; }

        public string ColumnName { get; }

        public string TitleNullable { get; }

        public string NullableText { get; }

        public string Type { get; }

        public string DefaultValue { get; }

        public HtmlString ColumnClass { get; }

        public HtmlString ColumnIcon { get; }

        public string ColumnTitle { get; }

        public IEnumerable<ParentKey> ParentKeys { get; }

        public uint ParentKeysCount { get; }

        public IEnumerable<ChildKey> ChildKeys { get; }

        public uint ChildKeysCount { get; }

        private static HtmlString BuildColumnClass(bool isPrimaryKeyColumn, bool isUniqueKeyColumn, bool isForeignKeyColumn)
        {
            var isKey = isPrimaryKeyColumn || isUniqueKeyColumn || isForeignKeyColumn;
            return isKey ? @"class=""is-key-column""" : string.Empty;
        }

        private static HtmlString BuildColumnIcon(bool isPrimaryKeyColumn, bool isUniqueKeyColumn, bool isForeignKeyColumn)
        {
            var iconPieces = new List<string>();

            if (isPrimaryKeyColumn)
            {
                const string iconText = @"<i title=""Primary Key"" class=""fa fa-key icon-primary-key"" style=""padding-left: 5px; padding-right: 5px;"" aria-hidden=""true""></i>";
                iconPieces.Add(iconText);
            }

            if (isUniqueKeyColumn)
            {
                const string iconText = @"<i title=""Unique Key"" class=""fa fa-key icon-unique-key"" style=""padding-left: 5px; padding-right: 5px;"" aria-hidden=""true""></i>";
                iconPieces.Add(iconText);
            }

            if (isForeignKeyColumn)
            {
                const string iconText = @"<i title=""Foreign Key"" class=""fa fa-key icon-foreign-key"" style=""padding-left: 5px; padding-right: 5px;"" aria-hidden=""true""></i>";
                iconPieces.Add(iconText);
            }

            return string.Concat(iconPieces);
        }

        private static string BuildColumnTitle(bool isPrimaryKeyColumn, bool isUniqueKeyColumn, bool isForeignKeyColumn)
        {
            var titlePieces = new List<string>();

            if (isPrimaryKeyColumn)
                titlePieces.Add("Primary Key");
            if (isUniqueKeyColumn)
                titlePieces.Add("Unique Key");
            if (isForeignKeyColumn)
                titlePieces.Add("Foreign Key");

            return titlePieces.Join(", ");
        }
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
            ReferentialAction updateAction,
            string rootPath
        ) : base(constraintName)
        {
            if (columnNames.NullOrEmpty())
                throw new ArgumentNullException(nameof(columnNames));
            if (parentTableName == null)
                throw new ArgumentNullException(nameof(parentTableName));
            if (parentColumnNames.NullOrEmpty())
                throw new ArgumentNullException(nameof(parentColumnNames));
            if (!deleteAction.IsValid())
                throw new ArgumentException($"The { nameof(ReferentialAction) } provided must be a valid enum.", nameof(deleteAction));
            if (!updateAction.IsValid())
                throw new ArgumentException($"The { nameof(ReferentialAction) } provided must be a valid enum.", nameof(updateAction));
            if (rootPath == null)
                throw new ArgumentNullException(nameof(rootPath));

            ChildColumnNames = columnNames.Join(", ");
            ParentConstraintName = parentConstraintName;
            ParentTableName = parentTableName.ToVisibleName();
            ParentTableUrl = rootPath + UrlRouter.GetTableUrl(parentTableName);
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
            [ReferentialAction.SetNull] = "SET NULL"
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
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

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
            UniqueText = isUnique ? "✓" : "✗";

            ColumnsText = columnNames.Zip(
                columnSorts.Select(SortToString),
                static (c, s) => c + " " + s
            ).Join(", ");
            IncludedColumnsText = includedColumnNames.Join(", ");
        }

        public string Name { get; }

        public string UniqueText { get; }

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
            Identifier tableName,
            Identifier triggerName,
            string definition,
            TriggerQueryTiming queryTiming,
            TriggerEvent triggerEvent
        )
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (triggerName == null)
                throw new ArgumentNullException(nameof(triggerName));
            if (definition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(definition));

            TriggerName = triggerName.LocalName;
            Definition = definition;

            TriggerUrl = "../" + UrlRouter.GetTriggerUrl(tableName, triggerName);

            var queryFlags = queryTiming.GetFlags()
                .Select(static qt => TimingDescriptions[qt])
                .OrderBy(static qt => qt)
                .ToList();
            var eventFlags = triggerEvent.GetFlags()
                .Select(static te => EventDescriptions[te])
                .OrderBy(static te => te)
                .ToList();

            QueryTiming = queryFlags.Join(", ");
            Events = eventFlags.Join(", ");
        }

        public string TriggerName { get; }

        public string TriggerUrl { get; }

        public string Definition { get; }

        public string QueryTiming { get; }

        public string Events { get; }

        private static readonly IReadOnlyDictionary<TriggerQueryTiming, string> TimingDescriptions = new Dictionary<TriggerQueryTiming, string>
        {
            [TriggerQueryTiming.After] = "AFTER",
            [TriggerQueryTiming.Before] = "BEFORE",
            [TriggerQueryTiming.InsteadOf] = "INSTEAD OF"
        };

        private static readonly IReadOnlyDictionary<TriggerEvent, string> EventDescriptions = new Dictionary<TriggerEvent, string>
        {
            [TriggerEvent.Delete] = "DELETE",
            [TriggerEvent.Insert] = "INSERT",
            [TriggerEvent.Update] = "UPDATE"
        };
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class ParentKey
    {
        public ParentKey(string constraintName, Identifier parentTableName, string parentColumnName, string qualifiedChildColumnName, string rootPath)
        {
            if (parentTableName == null)
                throw new ArgumentNullException(nameof(parentTableName));
            if (parentColumnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(parentColumnName));
            if (qualifiedChildColumnName.IsNullOrWhiteSpace())
                throw new ArgumentOutOfRangeException(nameof(qualifiedChildColumnName));
            if (rootPath == null)
                throw new ArgumentNullException(nameof(rootPath));

            ParentTableName = parentTableName.ToVisibleName();
            ParentTableUrl = rootPath + UrlRouter.GetTableUrl(parentTableName);
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
        public ChildKey(string constraintName, Identifier childTableName, string childColumnName, string qualifiedParentColumnName, string rootPath)
        {
            if (childTableName == null)
                throw new ArgumentNullException(nameof(childTableName));
            if (childColumnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(childColumnName));
            if (qualifiedParentColumnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(qualifiedParentColumnName));
            if (rootPath == null)
                throw new ArgumentNullException(nameof(rootPath));

            ChildTableName = childTableName.ToVisibleName();
            ChildTableUrl = rootPath + UrlRouter.GetTableUrl(childTableName);
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
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Diagram
    {
        public Diagram(Identifier tableName, string diagramName, string dotDefinition, bool isActive)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            if (diagramName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(diagramName));
            Name = diagramName;

            if (dotDefinition.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(dotDefinition));
            Dot = dotDefinition;

            ContainerId = tableName.ToSafeKey() + "-" + Name.ToLowerInvariant().Replace(' ', '-') + "-chart";
            ActiveClass = isActive ? "active" : string.Empty;
            Selected = isActive ? "true" : "false";
        }

        public string Name { get; }

        public string ContainerId { get; }

        public string ActiveClass { get; }

        public string Selected { get; }

        public string Dot { get; }

        // a bit hacky, needed to render image directly instead of via file
        public string Svg { get; set; } = string.Empty;
    }
}