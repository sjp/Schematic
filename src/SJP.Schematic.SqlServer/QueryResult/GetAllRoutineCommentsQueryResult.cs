using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetAllRoutineCommentsQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
