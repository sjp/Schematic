﻿using SJP.Schematic.Core;

namespace SJP.Schematic.SqlServer.Query
{
    public class IdentifierDefaults : IIdentifierDefaults
    {
        public string Server { get; set; }

        public string Database { get; set; }

        public string Schema { get; set; }
    }
}
