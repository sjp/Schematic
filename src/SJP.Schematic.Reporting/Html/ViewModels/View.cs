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
        IEnumerable<ReferencedObject> referencedObjects
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
    }

    public string Name { get; }

    public string ViewUrl { get; }

    public string Definition { get; }

    public IEnumerable<ViewColumn> Columns { get; }

    public uint ColumnsCount { get; }

    public IEnumerable<ReferencedObject> ReferencedObjects { get; }

    public uint ReferencedObjectsCount { get; }

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
