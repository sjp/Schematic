namespace SJP.Schematic.Sqlite.Query;

internal sealed record GetViewDefinitionQuery
{
    public string ViewName { get; set; } = default!;
}
