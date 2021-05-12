using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SJP.Schematic.Oracle.QueryResult
{
    internal sealed record GetTableChildKeysQueryResult
    {
        public string? ChildTableSchema { get; init; }

        public string? ChildTableName { get; init; }

        public string? ChildKeyName { get; init; }

        public string? EnabledStatus { get; init; }

        public string? DeleteAction { get; init; }

        public string? ParentKeyName { get; init; }

        public string? ParentKeyType { get; init; }
    }
}
