namespace SJP.Schematic.Oracle.Query;

internal sealed record GetUserTableCommentsQuery
{
    public string TableName { get; init; } = default!;
}