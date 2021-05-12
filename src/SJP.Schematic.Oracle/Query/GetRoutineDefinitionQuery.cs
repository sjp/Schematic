using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.Query
{
    internal sealed record GetRoutineDefinitionQuery
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }
}
