namespace SJP.Schematic.MySql.Query
{
    internal sealed record GetAllRoutineNamesQuery
    {
        public string SchemaName { get; init; } = default!;
    }
}
