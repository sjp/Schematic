using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    internal class Indexes : ITemplateParameter
    {
        public ReportTemplate Template { get; } = ReportTemplate.Indexes;

        public string RootPath
        {
            get => _rootPath;
            set => _rootPath = value ?? throw new ArgumentNullException(nameof(value));
        }

        private string _rootPath = "/";

        public IEnumerable<Index> TableIndexes
        {
            get => _indexes;
            set => _indexes = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Index> _indexes = Enumerable.Empty<Index>();

        public uint IndexesCount => TableIndexes.UCount();

        public string IndexesTableClass => IndexesCount > 0 ? CssClasses.DataTableClass : string.Empty;

        internal class Index
        {
            public Index(Identifier table)
            {
                Table = table ?? throw new ArgumentNullException(nameof(table));
            }

            public string Name { get; set; }

            protected Identifier Table { get; }

            public string TableName => Table.ToVisibleName();

            public string TableUrl => Table.ToSafeKey();

            public bool Unique { get; set; }

            public string UniqueText => Unique ? "✓" : string.Empty;

            public IEnumerable<string> Columns
            {
                get => _columns;
                set => _columns = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<string> _columns = Enumerable.Empty<string>();

            public IEnumerable<IndexColumnOrder> ColumnSorts
            {
                get => _columnSorts;
                set => _columnSorts = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<IndexColumnOrder> _columnSorts = Enumerable.Empty<IndexColumnOrder>();

            public IEnumerable<string> IncludedColumns
            {
                get => _includedColumns;
                set => _includedColumns = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<string> _includedColumns = Enumerable.Empty<string>();

            public string ColumnsText
            {
                get
                {
                    return Columns.Zip(
                        ColumnSorts.Select(SortToString),
                        (c, s) => c + " " + s
                    ).Join(", ");
                }
            }

            public string IncludedColumnsText => IncludedColumns.Join(", ");

            private static string SortToString(IndexColumnOrder order)
            {
                return order == IndexColumnOrder.Ascending
                    ? "ASC"
                    : "DESC";
            }
        }
    }
}
