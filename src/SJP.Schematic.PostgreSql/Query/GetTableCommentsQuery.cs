namespace SJP.Schematic.PostgreSql.Query
{
    internal sealed record GetTableCommentsQuery
    {
        public string SchemaName { get; init; } = default!;

        public string TableName { get; init; } = default!;
    }
}
