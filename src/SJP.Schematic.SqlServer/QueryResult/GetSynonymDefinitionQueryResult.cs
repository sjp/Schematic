using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetSynonymDefinitionQueryResult
    {
        public string? TargetServerName { get; init; }

        public string? TargetDatabaseName { get; init; }

        public string? TargetSchemaName { get; init; }

        public string TargetObjectName { get; init; } = default!;
    }
}
