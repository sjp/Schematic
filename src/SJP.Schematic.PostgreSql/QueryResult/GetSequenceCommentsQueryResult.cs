using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.PostgreSql.QueryResult
{
    internal sealed record GetSequenceCommentsQueryResult
    {
        public string? Comment { get; init; }
    }
}
