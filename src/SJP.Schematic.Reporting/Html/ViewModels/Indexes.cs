using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The indexes summary payload (<c>data/indexes.json</c>): every index across all tables, each with
/// a hash-route link to its owning table.
/// </summary>
public sealed class Indexes
{
    public Indexes(IEnumerable<IndexRow> indexes)
    {
        TableIndexes = indexes ?? throw new ArgumentNullException(nameof(indexes));
        IndexesCount = indexes.UCount();
    }

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
            IEnumerable<string> includedColumnNames,
            IndexType indexType,
            Option<string> filterDefinition,
            bool isEnabled,
            bool isValid,
            bool isVisible
        )
        {
            ArgumentNullException.ThrowIfNull(tableName);
            ArgumentNullException.ThrowIfNull(columnNames);
            if (columnNames.Empty())
                throw new ArgumentException("An index must have at least one column.", nameof(columnNames));
            ArgumentNullException.ThrowIfNull(columnSorts);
            if (columnSorts.Empty())
                throw new ArgumentException("An index must have at least one column sort.", nameof(columnSorts));
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

            IndexType = IndexTypeNames.GetName(indexType);
            FilterText = filterDefinition.Match(static filter => filter ?? string.Empty, static () => string.Empty);
            IsEnabled = isEnabled;
            IsValid = isValid;
            IsVisible = isVisible;
        }

        public string Name { get; }

        public string TableName { get; }

        public string TableUrl { get; }

        public bool IsUnique { get; }

        public string ColumnsText { get; }

        public string IncludedColumnsText { get; }

        public string IndexType { get; }

        public string FilterText { get; }

        public bool IsEnabled { get; }

        public bool IsValid { get; }

        public bool IsVisible { get; }

        private static string SortToString(IndexColumnOrder order)
        {
            return order == IndexColumnOrder.Ascending
                ? "ASC"
                : "DESC";
        }
    }
}
