namespace SJP.Schematic.Oracle.QueryResult;

internal sealed record GetIdentifierDefaultsQueryResult
{
    public string? ServerHost { get; init; }

    public string? ServerSid { get; init; }

    public string? DatabaseName { get; init; }

    public string? DefaultSchema { get; init; }
}
