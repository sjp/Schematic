namespace SJP.Schematic.MySql.Query;

internal sealed record GetSequenceNameQuery
{
    public string SchemaName { get; init; } = default!;

    public string SequenceName { get; init; } = default!;
}