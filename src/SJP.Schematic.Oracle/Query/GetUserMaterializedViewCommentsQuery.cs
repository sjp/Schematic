using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.Query
{
    internal sealed record GetUserMaterializedViewCommentsQuery
    {
        public string ViewName { get; init; } = default!;
    }
}
