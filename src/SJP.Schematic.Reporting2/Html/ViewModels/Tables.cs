using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The tables summary payload (<c>data/tables.json</c>): the full list of tables for the
/// client-side <c>DataTable</c> on the Tables page.
/// </summary>
public sealed class Tables : ITemplateParameter
{
    public Tables(IReadOnlyCollection<Main.Table> tables)
    {
        if (tables.NullOrAnyNull())
            throw new ArgumentNullException(nameof(tables));

        TablesCount = (uint)tables.Count;
        AllTables = tables;
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Tables;

    public uint TablesCount { get; }

    public IReadOnlyCollection<Main.Table> AllTables { get; }
}
