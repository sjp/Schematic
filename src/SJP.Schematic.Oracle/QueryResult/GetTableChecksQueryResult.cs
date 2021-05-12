using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetTableChecksQueryResult
    {
        public string ConstraintName { get; init; } = default!;

        public string? Definition { get; init; }

        public string EnabledStatus { get; init; } = default!;
    }
}
