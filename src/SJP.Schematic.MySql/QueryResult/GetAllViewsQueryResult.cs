using SJP.Schematic.Core;

namespace SJP.Schematic.MySql.QueryResult
{

    internal sealed record GetAllViewsQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;

        public string ObjectType { get; init; } = default!;

        public string ObjectName { get; init; } = default!;

        public string? Comment { get; init; }
    }
}
