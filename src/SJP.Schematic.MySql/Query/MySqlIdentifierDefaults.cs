using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Query
{
    internal sealed record MySqlIdentifierDefaults : IIdentifierDefaults
    {
        public string Server { get; init; } = default!;

        public string Database { get; init; } = default!;

        public string Schema { get; init; } = default!;
    }
}
