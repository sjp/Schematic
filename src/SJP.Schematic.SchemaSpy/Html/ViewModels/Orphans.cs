using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.SchemaSpy.Html.ViewModels
{
    internal class Orphans : ITemplateParameter
    {
        public SchemaSpyTemplate Template { get; } = SchemaSpyTemplate.Orphans;

        public IEnumerable<Table> Tables
        {
            get => _tables;
            set => _tables = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Table> _tables = Enumerable.Empty<Table>();

        public uint TablesCount => Tables.UCount();

        public string TablesTableClass => TablesCount > 0 ? CssClasses.DataTableClass : string.Empty;

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

            public uint ColumnCount { get; set; }

            public ulong RowCount { get; set; }
        }
    }
}
