using System;
using LanguageExt;

namespace SJP.Schematic.Sqlite.Parsing;

/// <summary>
/// A parsed check constraint definition.
/// </summary>
public class Check
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Check"/> class.
    /// </summary>
    /// <param name="constraintName">A parsed constraint name.</param>
    /// <param name="definition">The check definition.</param>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is <see langword="null" />, empty or whitespace.</exception>
    public Check(Option<string> constraintName, string definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);

        Name = constraintName;
        Definition = definition;
    }

    /// <summary>
    /// The parsed check constraint name.
    /// </summary>
    /// <value>A constraint name.</value>
    public Option<string> Name { get; }

    /// <summary>
    /// The parsed check constraint definition.
    /// </summary>
    /// <value>A check constraint definition.</value>
    public string Definition { get; }
}
