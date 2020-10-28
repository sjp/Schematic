using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Query
{
    internal sealed class SqlIdentifierDefaults : IIdentifierDefaults
    {
        public string Server { get; set; } = default!;

        public string Database { get; set; } = default!;

        public string Schema { get; set; } = default!;
    }
}
