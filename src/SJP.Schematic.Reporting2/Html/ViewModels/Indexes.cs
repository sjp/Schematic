using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The indexes summary payload (<c>data/indexes.json</c>): every index across all tables, each with
/// a hash-route link to its owning table.
/// </summary>
public sealed class Indexes : ITemplateParameter
{
    public Indexes(IEnumerable<IndexRow> indexes)
    {
        TableIndexes = indexes ?? throw new ArgumentNullException(nameof(indexes));
        IndexesCount = indexes.UCount();
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Indexes;

    public IEnumerable<IndexRow> TableIndexes { get; }

    public uint IndexesCount { get; }

    public sealed class IndexRow
    {
        public IndexRow(
            string? indexName,
            Identifier tableName,
            bool isUnique,
            IEnumerable<string> columnNames,
            IEnumerable<IndexColumnOrder> columnSorts,
            IEnumerable<string> includedColumnNames
        )
        {
            ArgumentNullException.ThrowIfNull(tableName);
            if (columnNames.NullOrEmpty())
                throw new ArgumentNullException(nameof(columnNames));
            if (columnSorts.NullOrEmpty())
                throw new ArgumentNullException(nameof(columnSorts));
            ArgumentNullException.ThrowIfNull(includedColumnNames);

            Name = indexName ?? string.Empty;
            TableName = tableName.ToVisibleName();
            TableUrl = UrlRouter.GetTableUrl(tableName);

            IsUnique = isUnique;

            ColumnsText = columnNames.Zip(
                columnSorts.Select(SortToString),
                static (c, s) => c + " " + s
            ).Join(", ");
            IncludedColumnsText = includedColumnNames.Join(", ");
        }

        public string Name { get; }

        public string TableName { get; }

        public string TableUrl { get; }

        public bool IsUnique { get; }

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
