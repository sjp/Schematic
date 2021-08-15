namespace SJP.Schematic.SqlServer.QueryResult
{
    internal sealed record GetAllSequenceNamesQueryResult
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;
    }
}
