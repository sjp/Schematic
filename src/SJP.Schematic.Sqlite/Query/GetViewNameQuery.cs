namespace SJP.Schematic.Sqlite.Query;

internal sealed record GetViewNameQuery
{
    public string ViewName { get; set; } = default!;
}