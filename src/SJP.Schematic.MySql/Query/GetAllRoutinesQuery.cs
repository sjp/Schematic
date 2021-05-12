using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Query
{
    internal sealed record GetAllRoutinesQuery
    {
        public string SchemaName { get; init; } = default!;
    }
}
