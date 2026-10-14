namespace SJP.Schematic.Oracle.Queries;

/// <summary>
/// A row of a standalone routine's definition: either one line of its source, or one of its
/// arguments. Source rows carry <see cref="LineNumber"/> and argument rows carry <see cref="Position"/>,
/// and the query returns all source rows first, in line order, followed by the arguments in signature order.
/// </summary>
internal sealed record RoutineDefinitionRow
{
    /// <summary>
    /// <c>FUNCTION</c> or <c>PROCEDURE</c>, on source rows only.
    /// </summary>
    public required string? RoutineType { get; init; }

    public required int? LineNumber { get; init; }

    public required string? Text { get; init; }

    /// <summary>
    /// The argument's position in the signature, on argument rows only. Zero is a function's return value.
    /// </summary>
    public required int? Position { get; init; }

    public required string? ArgumentName { get; init; }

    /// <summary>
    /// <c>IN</c>, <c>OUT</c> or <c>IN/OUT</c>.
    /// </summary>
    public required string? InOut { get; init; }

    public required string? ArgumentTypeSchema { get; init; }

    public required string? ArgumentTypeName { get; init; }

    public required int DataLength { get; init; }

    public required int? Precision { get; init; }

    public required int? Scale { get; init; }

    /// <summary>
    /// <c>Y</c> when the argument has a default. Oracle exposes the default's text as a
    /// <c>LONG</c>, which cannot be read alongside the rest of the row, so only its presence
    /// is available here.
    /// </summary>
    public required string? Defaulted { get; init; }
}
