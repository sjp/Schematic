namespace SJP.Schematic.PostgreSql.Query;

internal sealed record GetSequenceCommentsQuery
{
    public string SchemaName { get; init; } = default!;

    public string SequenceName { get; init; } = default!;
}