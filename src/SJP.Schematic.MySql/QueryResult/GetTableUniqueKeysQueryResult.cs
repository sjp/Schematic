namespace SJP.Schematic.MySql.QueryResult
{
    internal sealed record GetTableUniqueKeysQueryResult
    {
        public string ConstraintName { get; init; } = default!;

        public string ColumnName { get; init; } = default!;
    }
}
