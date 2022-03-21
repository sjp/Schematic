namespace SJP.Schematic.Oracle.Query;

internal sealed record GetUserViewCommentsQuery
{
    public string ViewName { get; init; } = default!;
}