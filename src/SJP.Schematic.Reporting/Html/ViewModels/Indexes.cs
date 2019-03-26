using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Indexes : ITemplateParameter
    {
        public Indexes(IEnumerable<Index> indexes)
        {
            TableIndexes = indexes ?? throw new ArgumentNullException(nameof(indexes));
            IndexesCount = indexes.UCount();
            IndexesTableClass = IndexesCount > 0 ? CssClasses.DataTableClass : string.Empty;
        }

        public ReportTemplate Template { get; } = ReportTemplate.Indexes;

        public IEnumerable<Index> TableIndexes { get; }

        public uint IndexesCount { get; }

        public HtmlString IndexesTableClass { get; }

        /// <summary>
        /// Internal. Not intended to be used outside of this assembly. Only required for templating.
        /// </summary>
        public sealed class Index
        {
            public Index(
                string indexName,
                Identifier tableName,
                bool isUnique,
                IEnumerable<string> columnNames,
                IEnumerable<IndexColumnOrder> columnSorts,
                IEnumerable<string> includedColumnNames
            )
            {
                if (tableName == null)
                    throw new ArgumentNullException(nameof(tableName));
                if (columnNames == null || columnNames.Empty())
                    throw new ArgumentNullException(nameof(columnNames));
                if (columnSorts == null || columnSorts.Empty())
                    throw new ArgumentNullException(nameof(columnSorts));
                if (includedColumnNames == null)
                    throw new ArgumentNullException(nameof(includedColumnNames));

                Name = indexName ?? string.Empty;
                TableName = tableName.ToVisibleName();
                TableUrl = UrlRouter.GetTableUrl(tableName);

                UniqueText = isUnique ? "✓" : "✗";

                ColumnsText = columnNames.Zip(
                    columnSorts.Select(SortToString),
                    (c, s) => c + " " + s
                ).Join(", ");
                IncludedColumnsText = includedColumnNames.Join(", ");
            }

            public string Name { get; }

            public string TableName { get; }

            public string TableUrl { get; }

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
    }
}
