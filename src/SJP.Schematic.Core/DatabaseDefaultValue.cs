using System;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// The default value applied to a column when an <c>INSERT</c> statement omits it.
/// </summary>
/// <seealso cref="IDatabaseDefaultValue" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseDefaultValue : IDatabaseDefaultValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseDefaultValue"/> class, whose expression
    /// has not been classified.
    /// </summary>
    /// <param name="definition">The default value expression, as the database reported it.</param>
    /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
    public DatabaseDefaultValue(string definition)
        : this(definition, DefaultValueKind.Unknown)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseDefaultValue"/> class.
    /// </summary>
    /// <param name="definition">The default value expression, as the database reported it.</param>
    /// <param name="kind">What the expression evaluates to.</param>
    /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace, or <paramref name="kind"/> is not a valid enum value.</exception>
    public DatabaseDefaultValue(string definition, DefaultValueKind kind)
        : this(definition, kind, Option<Identifier>.None, Option<Identifier>.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseDefaultValue"/> class.
    /// </summary>
    /// <param name="definition">The default value expression, as the database reported it.</param>
    /// <param name="kind">What the expression evaluates to.</param>
    /// <param name="constraintName">The name of the constraint carrying the default, if the database names one. Only the local name is kept.</param>
    /// <param name="sequenceName">The sequence the default draws from, if one was recognised.</param>
    /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace, or <paramref name="kind"/> is not a valid enum value.</exception>
    public DatabaseDefaultValue(string definition, DefaultValueKind kind, Option<Identifier> constraintName, Option<Identifier> sequenceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);
        if (!kind.IsValid())
            throw new ArgumentException($"The {nameof(DefaultValueKind)} provided must be a valid enum.", nameof(kind));

        Definition = definition;
        Kind = kind;
        ConstraintName = constraintName.Map(static name => Identifier.CreateQualifiedIdentifier(name.LocalName));
        SequenceName = kind == DefaultValueKind.SequenceNextValue ? sequenceName : Option<Identifier>.None;
    }

    /// <summary>
    /// The name of the constraint that carries the default, where the database models a default as
    /// a constraint in its own right.
    /// </summary>
    /// <value>A constraint name, if the database names one.</value>
    public Option<Identifier> ConstraintName { get; }

    /// <summary>
    /// The default value expression, exactly as the database reported it.
    /// </summary>
    /// <value>A default value expression.</value>
    public string Definition { get; }

    /// <summary>
    /// Describes what <see cref="Definition"/> evaluates to.
    /// </summary>
    /// <value>A default value classification.</value>
    public DefaultValueKind Kind { get; }

    /// <summary>
    /// The sequence the default draws its values from.
    /// </summary>
    /// <value>A sequence name, if one was recognised.</value>
    public Option<Identifier> SequenceName { get; }

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

            builder.Append("Default: ");
            ConstraintName.IfSome(name => builder.Append(name.LocalName).Append(" = "));
            builder.Append(Definition);

            return builder.GetStringAndRelease();
        }
    }
}
