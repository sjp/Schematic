using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.PostgreSql;

/// <summary>
/// A check constraint definition for use with PostgreSQL databases.
/// </summary>
/// <seealso cref="IDatabaseCheckConstraint" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class PostgreSqlCheckConstraint : IDatabaseCheckConstraint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlCheckConstraint"/> class.
    /// </summary>
    /// <param name="checkName">The name of the check constraint, if available.</param>
    /// <param name="definition">The constraint definition.</param>
    /// <exception cref="ArgumentNullException"><paramref name="checkName"/> or <paramref name="definition"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
    public PostgreSqlCheckConstraint(Identifier checkName, string definition)
        : this(checkName, definition, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlCheckConstraint"/> class.
    /// </summary>
    /// <param name="checkName">The name of the check constraint, if available.</param>
    /// <param name="definition">The constraint definition.</param>
    /// <param name="isValidated">Whether the existing rows have been verified against the constraint, i.e. <c>convalidated</c>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="checkName"/> or <paramref name="definition"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
    public PostgreSqlCheckConstraint(Identifier checkName, string definition, bool isValidated)
    {
        ArgumentNullException.ThrowIfNull(checkName);
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);

        Name = Option<Identifier>.Some(checkName.LocalName);
        Definition = definition;
        IsValidated = isValidated;
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
    /// Indicates whether the constraint is enabled. Always <see langword="true" />.
    /// </summary>
    /// <value>Always <see langword="true" />.</value>
    public bool IsEnabled { get; } = true;

    /// <summary>
    /// Indicates whether the existing rows have been verified against the constraint. A check
    /// declared or left <c>NOT VALID</c> reports <see langword="false" />.
    /// </summary>
    /// <value><see langword="true" /> if the constraint has been validated; otherwise, <see langword="false" />.</value>
    public bool IsValidated { get; }

    /// <summary>
    /// Always <see cref="ConstraintDeferrability.NotDeferrable"/>; PostgreSQL only accepts a
    /// <c>DEFERRABLE</c> clause on unique, primary and foreign key constraints.
    /// </summary>
    /// <value><see cref="ConstraintDeferrability.NotDeferrable"/>.</value>
    public ConstraintDeferrability Deferrability { get; } = ConstraintDeferrability.NotDeferrable;

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