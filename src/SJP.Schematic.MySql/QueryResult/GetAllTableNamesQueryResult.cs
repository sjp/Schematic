namespace SJP.Schematic.MySql.QueryResult
{
    internal sealed record GetAllTableNamesQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }
}
