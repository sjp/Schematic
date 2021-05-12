using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetAllTableNamesQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }
}
