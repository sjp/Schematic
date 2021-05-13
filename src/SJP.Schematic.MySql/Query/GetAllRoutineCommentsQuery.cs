namespace SJP.Schematic.MySql.Query
{
    internal sealed record GetAllRoutineCommentsQuery
    {
        public string SchemaName { get; init; } = default!;
    }
}
