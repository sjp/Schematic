using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Describes an autoincrementing sequence.
/// </summary>
public interface IAutoIncrement
{
    /// <summary>
    /// The starting value of the sequence.
    /// </summary>
    decimal InitialValue { get; }

    /// <summary>
    /// The value incremented to the current value for each new row.
    /// </summary>
    decimal Increment { get; }

    /// <summary>
    /// Describes whether a value supplied by an <c>INSERT</c> statement is accepted in place of a
    /// generated one.
    /// </summary>
    /// <value>A generation strategy, or <see cref="IdentityGeneration.Unknown"/> when the database does not report one.</value>
    IdentityGeneration Generation { get; }

    /// <summary>
    /// The smallest value the sequence generates.
    /// </summary>
    /// <value>A minimum value, if the database reports one.</value>
    Option<decimal> MinValue { get; }

    /// <summary>
    /// The largest value the sequence generates.
    /// </summary>
    /// <value>A maximum value, if the database reports one.</value>
    Option<decimal> MaxValue { get; }

    /// <summary>
    /// Whether the sequence restarts from its bound once exhausted, instead of failing.
    /// </summary>
    /// <value><see langword="true"/> if the sequence cycles; otherwise, <see langword="false"/>.</value>
    bool Cycle { get; }

    /// <summary>
    /// The sequence object backing the column, where the database implements the column with one.
    /// </summary>
    /// <value>A sequence name, if the column is backed by a sequence that the database names.</value>
    Option<Identifier> SequenceName { get; }
}
