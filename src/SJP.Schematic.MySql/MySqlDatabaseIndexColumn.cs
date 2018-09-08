using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.MySql
{
    public class MySqlDatabaseIndexColumn : IDatabaseIndexColumn
    {
        public MySqlDatabaseIndexColumn(IDatabaseColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            DependentColumns = new List<IDatabaseColumn> { column }.AsReadOnly();
        }

        public IReadOnlyList<IDatabaseColumn> DependentColumns { get; }

        public IndexColumnOrder Order { get; }

        public string GetExpression(IDatabaseDialect dialect)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));

            return DependentColumns
                .Select(c => dialect.QuoteName(c.Name))
                .Single();
        }
    }
}
