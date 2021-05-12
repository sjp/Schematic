using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetPackageDefinitionQueryResult
    {
        public string RoutineType { get; init; } = default!;

        public int LineNumber { get; init; }

        public string? Text { get; init; }
    }
}
