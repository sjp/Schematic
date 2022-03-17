using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database check constraint.
/// </summary>
/// <seealso cref="IDatabaseOptional" />
public interface IDatabaseCheckConstraint : IDatabaseOptional
{
    /// <summary>
    /// The check constraint name.
    /// </summary>
    /// <value>A constraint name, if available.</value>
    Option<Identifier> Name { get; }

    /// <summary>
    /// The definition of the check constraint.
    /// </summary>
    /// <value>The check constraint definition.</value>
    string Definition { get; }
}
