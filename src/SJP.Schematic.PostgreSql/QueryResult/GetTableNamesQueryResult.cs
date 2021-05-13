namespace SJP.Schematic.PostgreSql.QueryResult
{
    internal sealed record GetTableNamesQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }
}
