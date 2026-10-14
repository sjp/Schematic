namespace SJP.Schematic.Oracle.Queries;

/// <summary>
/// A trigger as read from <c>ALL_TRIGGERS</c>. The shape of a trigger is the same whether it is attached
/// to a table, a view or a materialized view, so all of them are mapped onto the core model from this row.
/// </summary>
internal sealed record TriggerRow
{
    public required string? TriggerSchema { get; init; }

    public required string? TriggerName { get; init; }

    public required string? TriggerType { get; init; }

    public required string? TriggerEvent { get; init; }

    public required string? Condition { get; init; }

    /// <summary>The comma-separated <c>UPDATE OF</c> column list, or <see langword="null" /> when there is none.</summary>
    public required string? UpdateColumns { get; init; }

    public required string? Definition { get; init; }

    public required string? EnabledStatus { get; init; }
}
