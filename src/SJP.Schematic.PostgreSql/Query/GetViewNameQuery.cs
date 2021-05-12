using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record GetViewNameQuery
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }
}
