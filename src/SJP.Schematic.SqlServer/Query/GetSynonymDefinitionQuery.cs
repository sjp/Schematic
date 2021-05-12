using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.SqlServer.Query
{
    internal sealed record GetSynonymDefinitionQuery
    {
        public string SchemaName { get; init; } = default!;

        public string SynonymName { get; init; } = default!;
    }
}
