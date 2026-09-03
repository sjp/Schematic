using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Describes the default value applied to a column when an <c>INSERT</c> statement omits it.
/// </summary>
public interface IDatabaseDefaultValue
{
    /// <summary>
    /// The name of the constraint that carries the default, where the database models a default as
    /// a constraint in its own right. Only SQL Server does; every other dialect reports
    /// <see cref="Option{A}.None"/>.
    /// </summary>
    /// <value>A constraint name, if the database names one.</value>
    Option<Identifier> ConstraintName { get; }

    /// <summary>
    /// The default value expression, exactly as the database reported it.
    /// </summary>
    /// <value>A default value expression.</value>
    string Definition { get; }

    /// <summary>
    /// Describes what <see cref="Definition"/> evaluates to.
    /// </summary>
    /// <value>A default value classification, or <see cref="DefaultValueKind.Unknown"/> when the
    /// dialect could not classify the expression.</value>
    DefaultValueKind Kind { get; }

    /// <summary>
    /// The sequence the default draws its values from. Only ever set when <see cref="Kind"/> is
    /// <see cref="DefaultValueKind.SequenceNextValue"/>, and even then only when the dialect can
    /// recover the name from the expression.
    /// </summary>
    /// <value>A sequence name, if one was recognised.</value>
    Option<Identifier> SequenceName { get; }
}
