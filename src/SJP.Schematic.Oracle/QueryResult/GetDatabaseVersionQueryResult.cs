using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetDatabaseVersionQueryResult
    {
        public string? ProductName { get; init; }

        public string? VersionNumber { get; init; }
    }
}
