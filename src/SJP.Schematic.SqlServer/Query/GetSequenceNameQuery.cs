using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record GetSequenceNameQuery
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;
    }
}
