using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record GetAllTableCommentsQuery
    {
        public string CommentProperty { get; init; } = default!;
    }
}
