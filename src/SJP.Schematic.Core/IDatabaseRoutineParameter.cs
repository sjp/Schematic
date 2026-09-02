using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a parameter declared by a database routine.
/// </summary>
public interface IDatabaseRoutineParameter
{
    /// <summary>
    /// The name of the parameter, when it has one.
    /// </summary>
    /// <value>
    /// A parameter name, or none when the parameter is positional. PostgreSQL arguments are
    /// declared without a name far more often than not, so this is a routine occurrence rather
    /// than an edge case.
    /// </value>
    Option<Identifier> Name { get; }

    /// <summary>
    /// The type of data the parameter accepts.
    /// </summary>
    /// <value>A data type.</value>
    IDbType Type { get; }

    /// <summary>
    /// The direction that values flow through the parameter.
    /// </summary>
    /// <value>A parameter direction.</value>
    RoutineParameterDirection Direction { get; }

    /// <summary>
    /// The expression applied when no value is provided for the parameter, if any.
    /// </summary>
    /// <value>
    /// A default value expression. None when the parameter has no default, and also when the
    /// database records that a default exists but does not expose its text.
    /// </value>
    Option<string> DefaultValue { get; }

    /// <summary>
    /// The one-based position of the parameter within the routine's signature.
    /// </summary>
    /// <value>An ordinal position.</value>
    int Ordinal { get; }
}
