using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.Query
{
    internal sealed class MySqlIdentifierDefaults : IIdentifierDefaults
    {
        public string? Server { get; set; }

        public string? Database { get; set; }

        public string? Schema { get; set; }
    }
}
