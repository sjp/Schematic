using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Internal. Not intended to be used outside of this assembly. Only required for templating.
/// </summary>
public sealed class Tables : ITemplateParameter
{
    public Tables(IEnumerable<Main.Table> tables)
    {
        if (tables.NullOrAnyNull())
            throw new ArgumentNullException(nameof(tables));

        TablesCount = tables.UCount();
        TablesTableClass = TablesCount > 0 ? CssClasses.DataTableClass : string.Empty;
        AllTables = tables;
    }

    public ReportTemplate Template { get; } = ReportTemplate.Tables;

    public uint TablesCount { get; }

    public HtmlString TablesTableClass { get; }

    public IEnumerable<Main.Table> AllTables { get; }
}