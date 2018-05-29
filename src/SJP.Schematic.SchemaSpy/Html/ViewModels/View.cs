using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.SchemaSpy.Html.ViewModels
{
    internal class View : ITemplateParameter
    {
        public SchemaSpyTemplate Template { get; } = SchemaSpyTemplate.View;

        public Identifier ViewName
        {
            get => _viewName;
            set => _viewName = value ?? throw new ArgumentNullException(nameof(value));
        }

        private Identifier _viewName;

        public string RootPath
        {
            get => _rootPath;
            set => _rootPath = value ?? throw new ArgumentNullException(nameof(value));
        }

        private string _rootPath = "../";

        public string Name => _viewName.ToVisibleName();

        public string ViewUrl => _viewName.ToSafeKey();

        public ulong RowCount { get; set; }

        public string Definition
        {
            get => _definition;
            set => _definition = value ?? string.Empty;
        }

        private string _definition = string.Empty;

        public IEnumerable<Column> Columns
        {
            get => _columns;
            set => _columns = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Column> _columns = Enumerable.Empty<Column>();

        public uint ColumnsCount => Columns.UCount();

        public string ColumnsTableClass => ColumnsCount > 0 ? CssClasses.DataTableClass : string.Empty;

        internal class Column
        {
            public Column(string columnName)
            {
                ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            }

            public int Ordinal { get; set; }

            public string ColumnName { get; }

            public string TitleNullable => IsNullable ? "Nullable" : string.Empty;

            public string NullableText => IsNullable ? "✓" : string.Empty;

            public bool IsNullable { get; set; }

            public string Type
            {
                get => _type;
                set => _type = value ?? string.Empty;
            }

            private string _type = string.Empty;

            public string DefaultValue
            {
                get => _defaultValue;
                set => _defaultValue = value ?? string.Empty;
            }

            private string _defaultValue = string.Empty;
        }
    }
}
