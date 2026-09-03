namespace SJP.Schematic.Core;

/// <summary>
/// Describes when a database checks a constraint relative to the statement that modified the data.
/// </summary>
public enum ConstraintDeferrability
{
    /// <summary>
    /// The constraint is always checked at the end of the statement that modified the data, and
    /// cannot be deferred. Databases that do not support deferrable constraints report this value.
    /// </summary>
    NotDeferrable,

    /// <summary>
    /// The constraint may be deferred, but is checked at the end of each statement unless a
    /// transaction defers it explicitly.
    /// </summary>
    DeferrableInitiallyImmediate,

    /// <summary>
    /// The constraint is deferred by default, and is only checked when the transaction commits.
    /// </summary>
    DeferrableInitiallyDeferred,
}
