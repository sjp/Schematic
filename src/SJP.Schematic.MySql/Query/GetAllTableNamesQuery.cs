using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Query
{
    internal sealed record GetAllTableNamesQuery
    {
        public string SchemaName { get; init; } = default!;
    }
}
