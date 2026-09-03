namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized description of where a table's superseded rows are retained.
/// </summary>
public sealed record TableSystemVersioning
{
    /// <summary>
    /// The name of the table holding rows superseded by later updates.
    /// </summary>
    public required Identifier HistoryTable { get; init; }

    /// <summary>
    /// The name of the column recording when a row's version became current.
    /// </summary>
    public required Identifier PeriodStartColumn { get; init; }

    /// <summary>
    /// The name of the column recording when a row's version stopped being current.
    /// </summary>
    public required Identifier PeriodEndColumn { get; init; }
}
