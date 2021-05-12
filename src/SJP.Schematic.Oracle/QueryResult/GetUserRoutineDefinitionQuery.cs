using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetUserRoutineDefinitionQuery
    {
        public string RoutineName { get; init; } = default!;
    }
}
