using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record SqlIdentifierDefaults : IIdentifierDefaults
    {
        public string Server { get; init; } = default!;

        public string Database { get; init; } = default!;

        public string Schema { get; init; } = default!;
    }
}
