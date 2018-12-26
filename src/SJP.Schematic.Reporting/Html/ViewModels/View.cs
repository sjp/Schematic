using System;
using System.Collections.Generic;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    public sealed class View : ITemplateParameter
    {
        public View(
            Identifier viewName,
            string rootPath,
            ulong rowCount,
            string definition,
            IEnumerable<Column> columns
        )
        {
            if (viewName == null)
                throw new ArgumentNullException(nameof(viewName));

            Name = viewName.ToVisibleName();
            ViewUrl = viewName.ToSafeKey();
            RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
            RowCount = rowCount;
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));

            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
            ColumnsCount = columns.UCount();
            ColumnsTableClass = ColumnsCount > 0 ? CssClasses.DataTableClass : string.Empty;
        }

        public ReportTemplate Template { get; } = ReportTemplate.View;

        public string RootPath { get; }

        public string Name { get; }

        public string ViewUrl { get; }

        public ulong RowCount { get; }

        public string Definition { get; }

        public IEnumerable<Column> Columns { get; }

        public uint ColumnsCount { get; }

        public HtmlString ColumnsTableClass { get; }

        public sealed class Column
        {
            public Column(
                string columnName,
                int ordinal,
                bool isNullable,
                string typeDefinition,
                string defaultValue
            )
            {
                ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
                Ordinal = ordinal;
                TitleNullable = isNullable ? "Nullable" : string.Empty;
                NullableText = isNullable ? "✓" : string.Empty;
                Type = typeDefinition ?? string.Empty;
                DefaultValue = defaultValue ?? string.Empty;
            }

            public int Ordinal { get; }

            public string ColumnName { get; }

            public string TitleNullable { get; }

            public string NullableText { get; }

            public string Type { get; }

            public string DefaultValue { get; }
        }
    }
}
