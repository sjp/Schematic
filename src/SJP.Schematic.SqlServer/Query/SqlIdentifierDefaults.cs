﻿using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Query
{
    internal class SqlIdentifierDefaults : IIdentifierDefaults
    {
        public string? Server { get; set; }

        public string? Database { get; set; }

        public string? Schema { get; set; }
    }
}
