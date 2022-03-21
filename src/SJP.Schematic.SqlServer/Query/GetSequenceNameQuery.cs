namespace SJP.Schematic.SqlServer.Query;

internal sealed record GetSequenceNameQuery
{
    public string SchemaName { get; init; } = default!;

    public string SequenceName { get; init; } = default!;
}