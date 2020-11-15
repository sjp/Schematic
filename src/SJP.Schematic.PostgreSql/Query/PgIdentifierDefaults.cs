using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record PgIdentifierDefaults : IIdentifierDefaults
    {
        public string? Server { get; init; }

        public string? Database { get; init; }

        public string? Schema { get; init; }
    }
}
