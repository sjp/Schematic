using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.SchemaSpy.Html.ViewModels
{
    internal class Main : ITemplateParameter
    {
        public SchemaSpyTemplate Template { get; } = SchemaSpyTemplate.Main;

        public string DatabaseName { get; set; }

        public string ProductName { get; set; }

        public string ProductVersion { get; set; }

        public DateTime GenerationTime => DateTime.Now;

        public uint TableCount { get; set; }

        public uint ViewCount { get; set; }

        public uint ColumnCount { get; set; }

        public uint ConstraintCount { get; set; }

        public IEnumerable<string> Schemas
        {
            get => _schemas;
            set => _schemas = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<string> _schemas = Enumerable.Empty<string>();

        public IEnumerable<Table> Tables
        {
            get => _tables;
            set => _tables = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Table> _tables = Enumerable.Empty<Table>();

        public IEnumerable<View> Views
        {
            get => _views;
            set => _views = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<View> _views = Enumerable.Empty<View>();

        internal class Table
        {
            public Table(Identifier tableName)
            {
                _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            }

            public Identifier TableName
            {
                get => _tableName;
                set => _tableName = value ?? throw new ArgumentNullException(nameof(value));
            }

            private Identifier _tableName;

            public string Name => _tableName.ToVisibleName();

            public string TableUrl => _tableName.ToSafeKey();

            public uint ParentsCount { get; set; }

            public uint ChildrenCount { get; set; }

            public uint ColumnCount { get; set; }

            public ulong RowCount { get; set; }
        }

        internal class View
        {
            public View(Identifier viewName)
            {
                _viewName = viewName ?? throw new ArgumentNullException(nameof(viewName));
            }

            public Identifier ViewName
            {
                get => _viewName;
                set => _viewName = value ?? throw new ArgumentNullException(nameof(value));
            }

            private Identifier _viewName;

            public string Name => _viewName.ToVisibleName();

            public string ViewUrl => _viewName.ToSafeKey();

            public uint ColumnCount { get; set; }

            public ulong RowCount { get; set; }
        }
    }
}
