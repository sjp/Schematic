using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.QueryResult
{
    internal sealed record PgIdentifierDefaultsQueryResult : IIdentifierDefaults
    {
        public string? Server { get; init; }

        public string? Database { get; init; }

        public string? Schema { get; init; }
    }
}
