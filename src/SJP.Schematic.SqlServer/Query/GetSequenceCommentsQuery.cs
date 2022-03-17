namespace SJP.Schematic.SqlServer.Query;

internal sealed record GetSequenceCommentsQuery
{
    public string SchemaName { get; init; } = default!;

    public string SequenceName { get; init; } = default!;

    public string CommentProperty { get; init; } = default!;
}
