using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The orphan-tables summary payload (<c>data/orphans.json</c>): tables that participate in no
/// relationships (no parent or child keys), each with a hash-route link to its detail page.
/// </summary>
public sealed class Orphans : ITemplateParameter
{
    public Orphans(IEnumerable<OrphanTable> tables)
    {
        Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        TablesCount = tables.UCount();
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Orphans;

    public IEnumerable<OrphanTable> Tables { get; }

    public uint TablesCount { get; }

    /// <summary>
    /// An orphan-table row. Named distinctly from <see cref="Main.Table"/> so the JSON source
    /// generator emits non-colliding metadata.
    /// </summary>
    public sealed class OrphanTable
    {
        public OrphanTable(Identifier tableName, uint columnCount, ulong rowCount)
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
