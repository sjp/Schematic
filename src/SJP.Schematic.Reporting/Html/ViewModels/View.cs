using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Internal. Not intended to be used outside of this assembly. Only required for templating.
/// </summary>
public sealed class View : ITemplateParameter
{
    public View(
        Identifier viewName,
        string rootPath,
        string definition,
        IEnumerable<Column> columns,
        IEnumerable<HtmlString> referencedObjects
    )
    {
        if (viewName == null)
            throw new ArgumentNullException(nameof(viewName));
        if (referencedObjects == null)
            throw new ArgumentNullException(nameof(referencedObjects));

        Name = viewName.ToVisibleName();
        ViewUrl = viewName.ToSafeKey();
        RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));

        Columns = columns ?? throw new ArgumentNullException(nameof(columns));
        ColumnsCount = columns.UCount();
        ColumnsTableClass = ColumnsCount > 0 ? CssClasses.DataTableClass : string.Empty;

        ReferencedObjects = new HtmlString(
            referencedObjects.Select(ro => ro.ToHtmlString()).Join(", ")
        );
        ReferencedObjectsCount = referencedObjects.UCount();
    }

    public ReportTemplate Template { get; } = ReportTemplate.View;

    public string RootPath { get; }

    public string Name { get; }

    public string ViewUrl { get; }

    public string Definition { get; }

    public IEnumerable<Column> Columns { get; }

    public uint ColumnsCount { get; }

    public HtmlString ColumnsTableClass { get; }

    public uint ReferencedObjectsCount { get; }

    public HtmlString ReferencedObjects { get; }

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
            Option<string> defaultValue
        )
        {
            ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            Ordinal = ordinal;
            TitleNullable = isNullable ? "Nullable" : string.Empty;
            NullableText = isNullable ? "✓" : "✗";
            Type = typeDefinition ?? string.Empty;
            DefaultValue = defaultValue.Match(static def => def ?? string.Empty, static () => string.Empty);
        }

        public int Ordinal { get; }

        public string ColumnName { get; }

        public string TitleNullable { get; }

        public string NullableText { get; }

        public string Type { get; }

        public string DefaultValue { get; }
    }
}
