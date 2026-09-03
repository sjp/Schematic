using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The per-view detail payload (<c>data/views/&lt;safeKey&gt;.json</c>): columns, the view
/// definition, and links to the objects the view references.
/// </summary>
public sealed class View
{
    public View(
        Identifier viewName,
        string definition,
        IEnumerable<ViewColumn> columns,
        IEnumerable<ReferencedObject> referencedObjects,
        IEnumerable<Table.Index> indexes,
        IEnumerable<Table.Trigger> triggers,
        ViewCheckOption checkOption,
        bool isUpdatable,
        bool isMaterialized,
        MaterializedViewRefreshMode refreshMode,
        Option<string> refreshMethod,
        bool isPopulated
    )
    {
        ArgumentNullException.ThrowIfNull(viewName);
        ArgumentNullException.ThrowIfNull(referencedObjects);

        Name = viewName.ToVisibleName();
        ViewUrl = UrlRouter.GetViewUrl(viewName);
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));

        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        ColumnsCount = columns.UCount();

        ReferencedObjects = referencedObjects;
        ReferencedObjectsCount = referencedObjects.UCount();

        Indexes = indexes ?? throw new ArgumentNullException(nameof(indexes));
        IndexesCount = indexes.UCount();

        Triggers = triggers ?? throw new ArgumentNullException(nameof(triggers));
        TriggersCount = triggers.UCount();

        CheckOption = GetCheckOptionDescription(checkOption);
        IsUpdatable = isUpdatable;

        IsMaterialized = isMaterialized;
        RefreshMode = GetRefreshModeDescription(refreshMode);
        RefreshMethod = refreshMethod.Match(static m => m ?? string.Empty, static () => string.Empty);
        IsPopulated = isPopulated;
    }

    public string Name { get; }

    public string ViewUrl { get; }

    public string Definition { get; }

    public IEnumerable<ViewColumn> Columns { get; }

    public uint ColumnsCount { get; }

    public IEnumerable<ReferencedObject> ReferencedObjects { get; }

    public uint ReferencedObjectsCount { get; }

    public IEnumerable<Table.Index> Indexes { get; }

    public uint IndexesCount { get; }

    public IEnumerable<Table.Trigger> Triggers { get; }

    public uint TriggersCount { get; }

    /// <summary>The view's check option, e.g. <c>WITH CASCADED CHECK OPTION</c>. Empty when it has none.</summary>
    public string CheckOption { get; }

    public bool IsUpdatable { get; }

    public bool IsMaterialized { get; }

    /// <summary>When a materialized view is refreshed, e.g. <c>ON DEMAND</c>. Empty when the view is not materialized, or the database did not report a refresh mode.</summary>
    public string RefreshMode { get; }

    /// <summary>How a materialized view is refreshed, e.g. <c>FAST</c>. Empty when the database has only one refresh method.</summary>
    public string RefreshMethod { get; }

    public bool IsPopulated { get; }

    private static string GetCheckOptionDescription(ViewCheckOption checkOption) => checkOption switch
    {
        ViewCheckOption.None => string.Empty,
        ViewCheckOption.Local => "WITH LOCAL CHECK OPTION",
        ViewCheckOption.Cascaded => "WITH CASCADED CHECK OPTION",
        _ => throw new ArgumentOutOfRangeException(nameof(checkOption)),
    };

    private static string GetRefreshModeDescription(MaterializedViewRefreshMode refreshMode) => refreshMode switch
    {
        MaterializedViewRefreshMode.Unknown => string.Empty,
        MaterializedViewRefreshMode.OnDemand => "ON DEMAND",
        MaterializedViewRefreshMode.OnCommit => "ON COMMIT",
        MaterializedViewRefreshMode.Never => "NEVER",
        _ => throw new ArgumentOutOfRangeException(nameof(refreshMode)),
    };

    /// <summary>
    /// A link from a view to an object it references (hash route into the SPA).
    /// </summary>
    public sealed class ReferencedObject
    {
        public ReferencedObject(string name, string url)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        public string Name { get; }

        public string Url { get; }
    }

    /// <summary>
    /// A column of the view (<c>data/views/&lt;safeKey&gt;.json</c>). Named distinctly from
    /// <see cref="Table.Column"/> so the JSON source generator emits non-colliding metadata.
    /// </summary>
    public sealed class ViewColumn
    {
        public ViewColumn(
            string columnName,
            int ordinal,
            bool isNullable,
            string typeDefinition,
            Option<string> defaultValue
        )
        {
            ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            Ordinal = ordinal;
            IsNullable = isNullable;
            Type = typeDefinition ?? string.Empty;
            DefaultValue = defaultValue.Match(static def => def ?? string.Empty, static () => string.Empty);
        }

        public int Ordinal { get; }

        public string ColumnName { get; }

        public bool IsNullable { get; }

        public string Type { get; }

        public string DefaultValue { get; }
    }
}
