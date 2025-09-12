using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// A check constraint definition.
/// </summary>
/// <seealso cref="IDatabaseCheckConstraint" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class SqliteCheckConstraint : IDatabaseCheckConstraint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteCheckConstraint"/> class.
    /// </summary>
    /// <param name="checkName">A check constraint name, if available.</param>
    /// <param name="definition">The constraint definition.</param>
    /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />, empty or whitespace.</exception>
    public SqliteCheckConstraint(Option<Identifier> checkName, string definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);

        Name = checkName.Map(static name => Identifier.CreateQualifiedIdentifier(name.LocalName));
        Definition = definition;
    }

    /// <summary>
    /// The check constraint name.
    /// </summary>
    /// <value>A constraint name, if available.</value>
    public Option<Identifier> Name { get; }

    /// <summary>
    /// The definition of the check constraint.
    /// </summary>
    /// <value>The check constraint definition.</value>
    public string Definition { get; }

    /// <summary>
    /// Indicates whether this constraint is enabled.
    /// </summary>
    /// <value>Always <see langword="true" />.</value>
    public bool IsEnabled { get; } = true;

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay
    {
        get
        {
            var builder = StringBuilderCache.Acquire();

            builder.Append("Check");

            Name.IfSome(name =>
            {
                builder.Append(": ")
                    .Append(name.LocalName);
            });

            return builder.GetStringAndRelease();
        }
    }
}