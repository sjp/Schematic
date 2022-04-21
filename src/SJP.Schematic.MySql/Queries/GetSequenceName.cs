namespace SJP.Schematic.MySql.Queries;

internal static class GetSequenceName
{
    internal sealed record Query
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;
    }

    internal sealed record Result
    {
        public string SchemaName { get; init; } = default!;

        public string SequenceName { get; init; } = default!;
    }
}