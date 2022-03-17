namespace SJP.Schematic.MySql.Query;

internal sealed record GetAllViewNamesQuery
{
    public string SchemaName { get; init; } = default!;
}
