﻿using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Internal. Not intended to be used outside of this assembly. Only required for templating.
/// </summary>
public sealed class Orphans : ITemplateParameter
{
    public Orphans(IEnumerable<Table> tables)
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        TablesCount = tables.UCount();
        TablesTableClass = TablesCount > 0 ? CssClasses.DataTableClass : string.Empty;
    }

    public ReportTemplate Template { get; } = ReportTemplate.Orphans;

    public IEnumerable<Table> Tables { get; }

    public uint TablesCount { get; }

    public HtmlString TablesTableClass { get; }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Table
    {
        public Table(Identifier tableName, uint columnCount, ulong rowCount)
        {
            ArgumentNullException.ThrowIfNull(tableName);

            Name = tableName.ToVisibleName();
            TableUrl = UrlRouter.GetTableUrl(tableName);
            ColumnCount = columnCount;
            RowCount = rowCount;
        }

        public string Name { get; }

        public string TableUrl { get; }

        public uint ColumnCount { get; }

        public ulong RowCount { get; }
    }
}