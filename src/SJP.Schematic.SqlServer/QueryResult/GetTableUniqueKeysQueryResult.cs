using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetTableUniqueKeysQueryResult
    {
        public string ConstraintName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;

        public bool IsDisabled { get; init; }
    }
}
