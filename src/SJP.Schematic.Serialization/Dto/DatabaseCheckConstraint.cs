namespace SJP.Schematic.Serialization.Dto;

/// <summary>
/// A serialized check constraint.
/// </summary>
public class DatabaseCheckConstraint
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
}
