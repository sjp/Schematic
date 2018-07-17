using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    internal class Orphans : ITemplateParameter
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

        public string TablesTableClass { get; }

        internal class Table
        {
            public Table(Identifier tableName, uint columnCount, ulong rowCount)
            {
                if (tableName == null)
                    throw new ArgumentNullException(nameof(tableName));

                Name = tableName.ToVisibleName();
                TableUrl = tableName.ToSafeKey();
                ColumnCount = columnCount;
                RowCount = rowCount;
            }

            public string Name { get; }

            public string TableUrl { get; }

            public uint ColumnCount { get; }

            public ulong RowCount { get; }
        }
    }
}
