using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The columns summary payload (<c>data/columns.json</c>): every column across all tables and views,
/// each carrying a hash-route link to its parent object and key-membership flags.
/// </summary>
public sealed class Columns
{
    public Columns(IEnumerable<ColumnSummary> columns)
    {
        TableColumns = columns ?? throw new ArgumentNullException(nameof(columns));
        ColumnsCount = columns.UCount();
    }

    public IEnumerable<ColumnSummary> TableColumns { get; }

    public uint ColumnsCount { get; }

    public enum ParentObjectType
    {
        None, // not intended to be used
        Table,
        View,
    }

    /// <summary>
    /// A column row in the columns summary list. Named distinctly from <see cref="Table.Column"/> so
    /// the JSON source generator emits non-colliding metadata. View columns carry all key flags as
    /// <c>false</c>.
    /// </summary>
    public sealed class ColumnSummary
    {
        public ColumnSummary(
            Identifier parentName,
            ParentObjectType parentType,
            string parentUrl,
            int ordinal,
            string columnName,
            string typeDefinition,
            bool isNullable,
            Option<string> defaultValue,
            bool isPrimaryKey,
            bool isUniqueKey,
            bool isForeignKey
        )
        {
            ArgumentNullException.ThrowIfNull(parentName);
            ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

            Name = parentName.ToVisibleName();
            ParentType = parentType;
            ParentUrl = parentUrl ?? throw new ArgumentNullException(nameof(parentUrl));
            Ordinal = ordinal;
            ColumnName = columnName;
            Type = typeDefinition ?? string.Empty;
            IsNullable = isNullable;
            DefaultValue = defaultValue.Match(static def => def ?? string.Empty, static () => string.Empty);
            IsPrimaryKey = isPrimaryKey;
            IsUniqueKey = isUniqueKey;
            IsForeignKey = isForeignKey;
        }

        public string Name { get; }

        public ParentObjectType ParentType { get; }

        public string ParentUrl { get; }

        public int Ordinal { get; }

        public string ColumnName { get; }

        public string Type { get; }

        public bool IsNullable { get; }

        public string DefaultValue { get; }

        public bool IsPrimaryKey { get; }

        public bool IsUniqueKey { get; }

        public bool IsForeignKey { get; }
    }
}
