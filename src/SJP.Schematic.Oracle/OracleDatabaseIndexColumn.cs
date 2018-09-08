using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseIndexColumn : IDatabaseIndexColumn
    {
        public OracleDatabaseIndexColumn(IDatabaseColumn column, IndexColumnOrder order)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (!order.IsValid())
                throw new ArgumentException($"The { nameof(IndexColumnOrder) } provided must be a valid enum.", nameof(order));

            DependentColumns = new List<IDatabaseColumn> { column }.AsReadOnly();
            Order = order;
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
