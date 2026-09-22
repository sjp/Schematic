using System;
using System.Collections.Generic;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;

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
            Option<Uri> typeUrl,
            bool isNullable,
            Option<string> defaultValue,
            bool isPrimaryKey,
            bool isUniqueKey,
            bool isForeignKey
        )
        {
            ArgumentNullException.ThrowIfNull(parentName);
            ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
            if (!parentType.IsValid())
                throw new ArgumentException($"The {nameof(ParentObjectType)} provided must be a valid enum.", nameof(parentType));
            if (parentType == ParentObjectType.None)
                throw new ArgumentException($"A column must belong to a table or a view, so {nameof(ParentObjectType)}.{nameof(ParentObjectType.None)} is not a valid parent.", nameof(parentType));

            Name = parentName.ToVisibleName();
            ParentType = parentType;
            ParentUrl = parentUrl ?? throw new ArgumentNullException(nameof(parentUrl));
            Ordinal = ordinal;
            ColumnName = columnName;
            Type = typeDefinition ?? string.Empty;
            TypeUrl = typeUrl.MatchUnsafe(static uri => uri.ToString(), static () => (string?)null);
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

        /// <summary>
        /// The hash route of the user-defined type the column is declared with. Omitted from the
        /// JSON when the column's type is not one of the report's user-defined types.
        /// </summary>
        public string? TypeUrl { get; }

        public bool IsNullable { get; }

        public string DefaultValue { get; }

        public bool IsPrimaryKey { get; }

        public bool IsUniqueKey { get; }

        public bool IsForeignKey { get; }
    }
}
