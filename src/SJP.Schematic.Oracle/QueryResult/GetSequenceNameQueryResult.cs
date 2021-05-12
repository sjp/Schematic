using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetSequenceNameQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;
    }
}
