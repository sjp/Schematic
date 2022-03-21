namespace SJP.Schematic.Oracle.Query;

internal sealed record GetUserMaterializedViewCommentsQuery
{
    public string ViewName { get; init; } = default!;
}