using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.PostgreSql.QueryResult
{
    internal sealed record GetTablePrimaryQueryResult
    {
        public string ConstraintName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;

        public int OrdinalPosition { get; init; }
    }
}
