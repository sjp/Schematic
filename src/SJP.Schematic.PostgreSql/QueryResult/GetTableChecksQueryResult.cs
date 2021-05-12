using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.PostgreSql.QueryResult
{
    internal sealed record GetTableChecksQueryResult
    {
        public string? ConstraintName { get; init; }

        public string? Definition { get; init; }
    }
}
