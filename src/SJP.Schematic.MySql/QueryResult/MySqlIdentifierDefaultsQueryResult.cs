using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.QueryResult
{
    internal sealed record MySqlIdentifierDefaultsQueryResult : IIdentifierDefaults
    {
        public string Server { get; init; } = default!;

        public string Database { get; init; } = default!;

        public string Schema { get; init; } = default!;
    }
}
