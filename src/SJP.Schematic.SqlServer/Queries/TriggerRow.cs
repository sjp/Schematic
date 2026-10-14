namespace SJP.Schematic.SqlServer.Queries;

/// <summary>
/// A trigger as read from the catalog. The shape of a trigger is the same whether it is attached to a
/// table or a view, so table and view triggers are both mapped onto the core model from this row.
/// </summary>
internal sealed record TriggerRow
{
    public required string TriggerName { get; init; }

    /// <summary>
    /// The trigger body, or <see langword="null" /> when the trigger was created <c>WITH ENCRYPTION</c>
    /// or the caller lacks <c>VIEW DEFINITION</c> on it.
    /// </summary>
    public required string? Definition { get; init; }

    public required bool IsInsteadOfTrigger { get; init; }

    public required bool IsDisabled { get; init; }

    public required bool IsInsertTrigger { get; init; }

    public required bool IsUpdateTrigger { get; init; }

    public required bool IsDeleteTrigger { get; init; }

    // Set when a trigger fires on an event other than INSERT/UPDATE/DELETE, i.e. one Schematic doesn't
    // currently model. The trigger is still returned, with TriggerEvent.Other set alongside whatever
    // events are modelled.
    public required bool IsOtherTrigger { get; init; }
}
