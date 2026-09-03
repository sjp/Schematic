namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized check constraint.
/// </summary>
public sealed record DatabaseCheckConstraint
{
    /// <summary>
    /// The name of the check constraint, if available.
    /// </summary>
    public Identifier? CheckName { get; init; }

    /// <summary>
    /// The expression that rows must satisfy.
    /// </summary>
    public required string Definition { get; init; }

    /// <summary>
    /// Whether the check constraint is enabled.
    /// </summary>
    public required bool IsEnabled { get; init; }

    /// <summary>
    /// Whether the existing rows are known to satisfy the check constraint. Defaults to
    /// <see langword="true"/> for a document written before checks carried a validation state.
    /// </summary>
    public bool IsValidated { get; init; } = true;

    /// <summary>
    /// When the database checks the constraint.
    /// </summary>
    /// <remarks>
    /// Not required, so that a document written before checks carried deferrability still reads back,
    /// as a constraint that cannot be deferred.
    /// </remarks>
    public Core.ConstraintDeferrability Deferrability { get; init; }
}
