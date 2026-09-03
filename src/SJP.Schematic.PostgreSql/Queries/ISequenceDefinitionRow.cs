namespace SJP.Schematic.PostgreSql.Queries;

/// <summary>
/// The sequence definition columns that are shared by the single-sequence and all-sequences
/// queries, so that both results are mapped onto the core model by the same code.
/// </summary>
internal interface ISequenceDefinitionRow
{
    string TypeName { get; }

    int CacheSize { get; }

    bool Cycle { get; }

    decimal Increment { get; }

    decimal MinValue { get; }

    decimal MaxValue { get; }

    decimal StartValue { get; }
}
