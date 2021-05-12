using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetAllViewCommentsQueryResult
    {
        public string ColumnName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
