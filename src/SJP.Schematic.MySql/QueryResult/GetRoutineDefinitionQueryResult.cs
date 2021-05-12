namespace SJP.Schematic.MySql.QueryResult
{
    internal sealed record GetRoutineDefinitionQueryResult
    {
        public string Definition { get; init; } = default!;
    }
}
