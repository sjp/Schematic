using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetTableTriggersQueryResult
    {
        public string TriggerName { get; init; } = default!;

        public string Definition { get; init; } = default!;

        public bool IsInsteadOfTrigger { get; init; }

        public string TriggerEvent { get; init; } = default!;

        public bool IsDisabled { get; init; }
    }
}
