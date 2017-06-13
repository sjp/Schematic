using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SJP.Schema.Core;

namespace SJP.Schema.Sqlite
{
    public class SqliteDatabaseIndexColumn : IDatabaseIndexColumn
    {
        public SqliteDatabaseIndexColumn(IDatabaseColumn column, IndexColumnOrder order)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var columns = new[] { column };
            DependentColumns = columns.ToImmutableList();
            Order = order;
        }

        public IList<IDatabaseColumn> DependentColumns { get; }

        public IndexColumnOrder Order { get; }

        public string GetExpression(IDatabaseDialect dialect)
        {
            return DependentColumns
                .Select(c => dialect.QuoteName(c.Name))
                .Single();
        }
    }
}
