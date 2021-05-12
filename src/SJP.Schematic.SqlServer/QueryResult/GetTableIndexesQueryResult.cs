using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetTableIndexesQueryResult
    {
        public string IndexName { get; init; } = default!;

        public bool IsUnique { get; init; }

        public int KeyOrdinal { get; init; }

        public int IndexColumnId { get; init; }

        public bool IsIncludedColumn { get; init; }

        public bool IsDescending { get; init; }

        public string ColumnName { get; init; } = default!;

        public bool IsDisabled { get; init; }
    }
}
