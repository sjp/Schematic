namespace SJP.Schematic.SqlServer.Queries;

internal static class GetMaterializedViewColumns
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string ViewName { get; init; } = default!;
    }
}