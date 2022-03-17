namespace SJP.Schematic.Oracle.Query;

internal sealed record GetViewCommentsQuery
{
    public string SchemaName { get; init; } = default!;

    public string ViewName { get; init; } = default!;
}
