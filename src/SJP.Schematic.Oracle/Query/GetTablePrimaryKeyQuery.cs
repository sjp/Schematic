using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.Query
{
    internal sealed record GetTablePrimaryKeyQuery
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }
}
