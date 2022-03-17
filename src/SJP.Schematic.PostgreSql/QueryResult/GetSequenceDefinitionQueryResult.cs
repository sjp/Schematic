namespace SJP.Schematic.PostgreSql.QueryResult;

internal sealed record GetSequenceDefinitionQueryResult
{
    public int CacheSize { get; init; }

    public bool Cycle { get; init; }

    public decimal Increment { get; init; }

    public decimal MinValue { get; init; }

    public decimal MaxValue { get; init; }

    public decimal StartValue { get; init; }
}
