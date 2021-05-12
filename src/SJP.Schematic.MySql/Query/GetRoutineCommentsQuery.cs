using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Query
{
    internal sealed record GetRoutineCommentsQuery
    {
        public string SchemaName { get; init; } = default!;

        public string RoutineName { get; init; } = default!;
    }
}
