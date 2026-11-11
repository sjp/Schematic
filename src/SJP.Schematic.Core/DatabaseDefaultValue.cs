using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// The default value applied to a column when an <c>INSERT</c> statement omits it, classified by
/// what its expression evaluates to.
/// </summary>
/// <remarks>
/// <para>
/// The members that every kind of default has are available directly. Switch on the default to
/// reach the members that only some kinds have, such as the
/// <see cref="SequenceNextValue.SequenceName"/> of a default drawn from a sequence.
/// </para>
/// <para>
/// <see cref="Unknown"/> is a classification in its own right rather than an error: a dialect
/// cannot always tell what an expression evaluates to, and a consumer may still inspect its text.
/// </para>
/// <para>
/// A <see langword="default"/> value holds no default, and every member throws when read from it.
/// </para>
/// </remarks>
[DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
public readonly union DatabaseDefaultValue(
    DatabaseDefaultValue.Unknown,
    DatabaseDefaultValue.Literal,
    DatabaseDefaultValue.Null,
    DatabaseDefaultValue.Expression,
    DatabaseDefaultValue.SequenceNextValue
)
{
    /// <summary>
    /// The default value expression, exactly as the database reported it.
    /// </summary>
    /// <value>A default value expression.</value>
    public string Definition => this switch
    {
        Unknown def => def.Definition,
        Literal def => def.Definition,
        Null def => def.Definition,
        Expression def => def.Definition,
        SequenceNextValue def => def.Definition,
    };

    /// <summary>
    /// The name of the constraint that carries the default, where the database models a default as
    /// a constraint in its own right. Only SQL Server does; every other dialect reports
    /// <see cref="Option{A}.None"/>.
    /// </summary>
    /// <value>A constraint name, if the database names one.</value>
    public Option<Identifier> ConstraintName => this switch
    {
        Unknown def => def.ConstraintName,
        Literal def => def.ConstraintName,
        Null def => def.ConstraintName,
        Expression def => def.ConstraintName,
        SequenceNextValue def => def.ConstraintName,
    };

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => Value?.ToString() ?? string.Empty;

    /// <summary>
    /// A default whose expression the dialect could not classify, either because the database does
    /// not report enough to do so, or because the dialect provider does not recognise its shape.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Unknown
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Unknown"/> class.
        /// </summary>
        /// <param name="definition">The default value expression, as the database reported it.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
        public Unknown(string definition)
            : this(definition, Option<Identifier>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Unknown"/> class.
        /// </summary>
        /// <param name="definition">The default value expression, as the database reported it.</param>
        /// <param name="constraintName">The name of the constraint carrying the default, if the database names one. Only the local name is kept.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
        public Unknown(string definition, Option<Identifier> constraintName)
        {
            Definition = DefaultValueArguments.ValidateDefinition(definition);
            ConstraintName = DefaultValueArguments.LocalName(constraintName);
        }

        /// <summary>
        /// The default value expression, exactly as the database reported it.
        /// </summary>
        /// <value>A default value expression.</value>
        public string Definition { get; }

        /// <summary>
        /// The name of the constraint that carries the default, where the database models a default
        /// as a constraint in its own right.
        /// </summary>
        /// <value>A constraint name, if the database names one.</value>
        public Option<Identifier> ConstraintName { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => DefaultValueArguments.Describe(ConstraintName, Definition);
    }

    /// <summary>
    /// A default whose expression is a constant, e.g. <c>0</c>, <c>'unassigned'</c> or <c>true</c>.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Literal
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Literal"/> class.
        /// </summary>
        /// <param name="definition">The default value expression, as the database reported it.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
        public Literal(string definition)
            : this(definition, Option<Identifier>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Literal"/> class.
        /// </summary>
        /// <param name="definition">The default value expression, as the database reported it.</param>
        /// <param name="constraintName">The name of the constraint carrying the default, if the database names one. Only the local name is kept.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
        public Literal(string definition, Option<Identifier> constraintName)
        {
            Definition = DefaultValueArguments.ValidateDefinition(definition);
            ConstraintName = DefaultValueArguments.LocalName(constraintName);
        }

        /// <summary>
        /// The default value expression, exactly as the database reported it.
        /// </summary>
        /// <value>A default value expression.</value>
        public string Definition { get; }

        /// <summary>
        /// The name of the constraint that carries the default, where the database models a default
        /// as a constraint in its own right.
        /// </summary>
        /// <value>A constraint name, if the database names one.</value>
        public Option<Identifier> ConstraintName { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => DefaultValueArguments.Describe(ConstraintName, Definition);
    }

    /// <summary>
    /// A default whose expression is the <c>NULL</c> literal. Kept apart from <see cref="Literal"/>
    /// because a default of <c>NULL</c> is the absence of a value rather than a value.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Null
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Null"/> class.
        /// </summary>
        /// <param name="definition">The default value expression, as the database reported it.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
        public Null(string definition)
            : this(definition, Option<Identifier>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Null"/> class.
        /// </summary>
        /// <param name="definition">The default value expression, as the database reported it.</param>
        /// <param name="constraintName">The name of the constraint carrying the default, if the database names one. Only the local name is kept.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
        public Null(string definition, Option<Identifier> constraintName)
        {
            Definition = DefaultValueArguments.ValidateDefinition(definition);
            ConstraintName = DefaultValueArguments.LocalName(constraintName);
        }

        /// <summary>
        /// The default value expression, exactly as the database reported it.
        /// </summary>
        /// <value>A default value expression.</value>
        public string Definition { get; }

        /// <summary>
        /// The name of the constraint that carries the default, where the database models a default
        /// as a constraint in its own right.
        /// </summary>
        /// <value>A constraint name, if the database names one.</value>
        public Option<Identifier> ConstraintName { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => DefaultValueArguments.Describe(ConstraintName, Definition);
    }

    /// <summary>
    /// A default whose expression is evaluated by the database for each row that omits the column,
    /// e.g. <c>getdate()</c> or <c>lower(current_user)</c>.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Expression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// </summary>
        /// <param name="definition">The default value expression, as the database reported it.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
        public Expression(string definition)
            : this(definition, Option<Identifier>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// </summary>
        /// <param name="definition">The default value expression, as the database reported it.</param>
        /// <param name="constraintName">The name of the constraint carrying the default, if the database names one. Only the local name is kept.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
        public Expression(string definition, Option<Identifier> constraintName)
        {
            Definition = DefaultValueArguments.ValidateDefinition(definition);
            ConstraintName = DefaultValueArguments.LocalName(constraintName);
        }

        /// <summary>
        /// The default value expression, exactly as the database reported it.
        /// </summary>
        /// <value>A default value expression.</value>
        public string Definition { get; }

        /// <summary>
        /// The name of the constraint that carries the default, where the database models a default
        /// as a constraint in its own right.
        /// </summary>
        /// <value>A constraint name, if the database names one.</value>
        public Option<Identifier> ConstraintName { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => DefaultValueArguments.Describe(ConstraintName, Definition);
    }

    /// <summary>
    /// A default whose expression draws the next value of a sequence, e.g. SQL Server
    /// <c>NEXT VALUE FOR</c>, PostgreSQL <c>nextval(...)</c> or Oracle <c>seq.NEXTVAL</c>.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class SequenceNextValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceNextValue"/> class, whose sequence
        /// could not be recovered from the expression.
        /// </summary>
        /// <param name="definition">The default value expression, as the database reported it.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
        public SequenceNextValue(string definition)
            : this(definition, Option<Identifier>.None, Option<Identifier>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceNextValue"/> class.
        /// </summary>
        /// <param name="definition">The default value expression, as the database reported it.</param>
        /// <param name="constraintName">The name of the constraint carrying the default, if the database names one. Only the local name is kept.</param>
        /// <param name="sequenceName">The sequence the default draws from, if one was recognised.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
        public SequenceNextValue(string definition, Option<Identifier> constraintName, Option<Identifier> sequenceName)
        {
            Definition = DefaultValueArguments.ValidateDefinition(definition);
            ConstraintName = DefaultValueArguments.LocalName(constraintName);
            SequenceName = sequenceName;
        }

        /// <summary>
        /// The default value expression, exactly as the database reported it.
        /// </summary>
        /// <value>A default value expression.</value>
        public string Definition { get; }

        /// <summary>
        /// The name of the constraint that carries the default, where the database models a default
        /// as a constraint in its own right.
        /// </summary>
        /// <value>A constraint name, if the database names one.</value>
        public Option<Identifier> ConstraintName { get; }

        /// <summary>
        /// The sequence the default draws its values from, when the dialect can recover the name
        /// from the expression.
        /// </summary>
        /// <value>A sequence name, if one was recognised.</value>
        public Option<Identifier> SequenceName { get; }

        /// <summary>
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => DefaultValueArguments.Describe(ConstraintName, Definition);
    }
}

/// <summary>
/// Argument handling shared by the default value cases.
/// </summary>
file static class DefaultValueArguments
{
    public static string ValidateDefinition(string definition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);

        return definition;
    }

    public static Option<Identifier> LocalName(Option<Identifier> name) => name.Map(static n => LocalIdentifier.From(n));

    public static string Describe(Option<Identifier> constraintName, string definition)
    {
        var builder = StringBuilderCache.Acquire();

        builder.Append("Default: ");
        constraintName.IfSome(name => builder.Append(name.LocalName).Append(" = "));
        builder.Append(definition);

        return builder.GetStringAndRelease();
    }
}
