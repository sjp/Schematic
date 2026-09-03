using System;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// Represents a database check constraint.
/// </summary>
/// <seealso cref="IDatabaseCheckConstraint" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseCheckConstraint : IDatabaseCheckConstraint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseCheckConstraint"/> class.
    /// </summary>
    /// <param name="checkName">The name of the check constraint, if available.</param>
    /// <param name="definition">The constraint definition.</param>
    /// <param name="isEnabled">Whether the constraint is enabled.</param>
    /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
    public DatabaseCheckConstraint(Option<Identifier> checkName, string definition, bool isEnabled)
        : this(checkName, definition, isEnabled, true, ConstraintDeferrability.NotDeferrable)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseCheckConstraint"/> class.
    /// </summary>
    /// <param name="checkName">The name of the check constraint, if available.</param>
    /// <param name="definition">The constraint definition.</param>
    /// <param name="isEnabled">Whether the constraint is enabled.</param>
    /// <param name="isValidated">Whether the existing rows are known to satisfy the constraint.</param>
    /// <param name="deferrability">When the database checks the constraint.</param>
    /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace, or <paramref name="deferrability"/> is an invalid enum value.</exception>
    public DatabaseCheckConstraint(Option<Identifier> checkName, string definition, bool isEnabled, bool isValidated, ConstraintDeferrability deferrability)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);
        if (!deferrability.IsValid())
            throw new ArgumentException($"The {nameof(ConstraintDeferrability)} provided must be a valid enum.", nameof(deferrability));

        Name = checkName.Map(name => Identifier.CreateQualifiedIdentifier(name.LocalName));
        Definition = definition;
        IsEnabled = isEnabled;
        IsValidated = isValidated;
        Deferrability = deferrability;
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
    /// Indicates whether the constraint is enabled.
    /// </summary>
    /// <value><see langword="true" /> if the constraint is enabled; otherwise, <see langword="false" />.</value>
    public bool IsEnabled { get; }

    /// <summary>
    /// Indicates whether the existing rows are known to satisfy the constraint.
    /// </summary>
    /// <value><see langword="true" /> if the constraint has been validated; otherwise, <see langword="false" />.</value>
    public bool IsValidated { get; }

    /// <summary>
    /// Describes when the database checks the constraint.
    /// </summary>
    /// <value>A deferrability value.</value>
    public ConstraintDeferrability Deferrability { get; }

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