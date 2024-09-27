using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Internal. Not intended to be used outside of this assembly. Only required for templating.
/// </summary>
public sealed class Columns : ITemplateParameter
{
    public Columns(IEnumerable<Column> columns)
    {
        TableColumns = columns ?? throw new ArgumentNullException(nameof(columns));
        ColumnsCount = columns.UCount();
        ColumnsTableClass = ColumnsCount > 0 ? CssClasses.DataTableClass : string.Empty;
    }

    public ReportTemplate Template { get; } = ReportTemplate.Columns;

    public IEnumerable<Column> TableColumns { get; }

    public uint ColumnsCount { get; }

    public HtmlString ColumnsTableClass { get; }

    public abstract class Column
    {
        protected Column(
            Identifier tableName,
            int ordinalPosition,
            string columnName,
            string typeDefinition,
            bool isNullable,
            Option<string> defaultValue
        )
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

                ColumnName = columnName;
            Name = tableName.ToVisibleName();
            Ordinal = ordinalPosition;
            TitleNullable = isNullable ? "Nullable" : string.Empty;
            NullableText = isNullable ? "✓" : "✗";
            Type = typeDefinition ?? string.Empty;
            DefaultValue = defaultValue.Match(static def => def ?? string.Empty, static () => string.Empty);
        }

        public string Name { get; }

        public abstract string TableUrl { get; }

        public string TableType => ParentType.ToString();

        public int Ordinal { get; }

        public string ColumnName { get; }

        public abstract ParentObjectType ParentType { get; }

        public string TitleNullable { get; }

        public string NullableText { get; }

        public string Type { get; }

        public virtual string DefaultValue { get; }

        public virtual HtmlString ColumnClass => string.Empty;

        public virtual HtmlString ColumnIcon { get; } = string.Empty;

        public virtual string ColumnTitle { get; } = string.Empty;

        public enum ParentObjectType
        {
            None, // not intended to be used
            Table,
            View
        }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class TableColumn : Column
    {
        public TableColumn(
            Identifier tableName,
            int ordinalPosition,
            string columnName,
            string typeDefinition,
            bool isNullable,
            Option<string> defaultValue,
            bool isPrimaryKeyColumn,
            bool isUniqueKeyColumn,
            bool isForeignKeyColumn
        ) : base(
            tableName,
            ordinalPosition,
            columnName,
            typeDefinition,
            isNullable,
            defaultValue
        )
        {
            TableUrl = UrlRouter.GetTableUrl(tableName);

            var isKey = isPrimaryKeyColumn || isUniqueKeyColumn || isForeignKeyColumn;
            ColumnClass = isKey ? @"class=""is-key-column""" : string.Empty;

            ColumnIcon = BuildColumnIcon(isPrimaryKeyColumn, isUniqueKeyColumn, isForeignKeyColumn);
            ColumnTitle = BuildColumnTitle(isPrimaryKeyColumn, isUniqueKeyColumn, isForeignKeyColumn);
        }

        public override string TableUrl { get; }

        public override ParentObjectType ParentType { get; } = ParentObjectType.Table;

        public override HtmlString ColumnClass { get; }

        public override HtmlString ColumnIcon { get; }

        public override string ColumnTitle { get; }

        private static string BuildColumnTitle(bool isPrimaryKeyColumn, bool isUniqueKeyColumn, bool isForeignKeyColumn)
        {
            var titlePieces = new List<string>();

            if (isPrimaryKeyColumn)
                titlePieces.Add("Primary Key");
            if (isUniqueKeyColumn)
                titlePieces.Add("Unique Key");
            if (isForeignKeyColumn)
                titlePieces.Add("Foreign Key");

            return titlePieces.Join(", ");
        }

        private static string BuildColumnIcon(bool isPrimaryKeyColumn, bool isUniqueKeyColumn, bool isForeignKeyColumn)
        {
            var iconPieces = new List<string>();

            if (isPrimaryKeyColumn)
            {
                const string iconText = @"<i title=""Primary Key"" class=""fa fa-key icon-primary-key"" style=""padding-left: 5px; padding-right: 5px;"" aria-hidden=""true""></i>";
                iconPieces.Add(iconText);
            }

            if (isUniqueKeyColumn)
            {
                const string iconText = @"<i title=""Unique Key"" class=""fa fa-key icon-unique-key"" style=""padding-left: 5px; padding-right: 5px;"" aria-hidden=""true""></i>";
                iconPieces.Add(iconText);
            }

            if (isForeignKeyColumn)
            {
                const string iconText = @"<i title=""Foreign Key"" class=""fa fa-key icon-foreign-key"" style=""padding-left: 5px; padding-right: 5px;"" aria-hidden=""true""></i>";
                iconPieces.Add(iconText);
            }

            return string.Concat(iconPieces);
        }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class ViewColumn : Column
    {
        public ViewColumn(
            Identifier viewName,
            int ordinalPosition,
            string columnName,
            string typeDefinition,
            bool isNullable
        ) : base(
            viewName,
            ordinalPosition,
            columnName,
            typeDefinition,
            isNullable,
            string.Empty
        )
        {
            TableUrl = UrlRouter.GetViewUrl(viewName);
        }

        public override string TableUrl { get; }

        public override ParentObjectType ParentType { get; } = ParentObjectType.View;
    }
}