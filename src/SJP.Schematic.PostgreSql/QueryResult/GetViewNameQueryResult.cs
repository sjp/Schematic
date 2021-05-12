using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.PostgreSql.QueryResult
{
    internal sealed record GetViewNameQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }
}
