using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetTableUniqueKeysQueryResult
    {
        public string? ConstraintName { get; init; }

        public string? EnabledStatus { get; init; }

        public string? ColumnName { get; init; }

        public int ColumnPosition { get; init; }
    }
}
