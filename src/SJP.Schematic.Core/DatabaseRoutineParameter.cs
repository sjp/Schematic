using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A parameter declared by a database routine.
/// </summary>
/// <seealso cref="IDatabaseRoutineParameter" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseRoutineParameter : IDatabaseRoutineParameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseRoutineParameter"/> class.
    /// </summary>
    /// <param name="parameterName">The parameter name, or none when the parameter is positional. Only the local name is kept.</param>
    /// <param name="type">The type of data the parameter accepts.</param>
    /// <param name="direction">The direction that values flow through the parameter.</param>
    /// <param name="defaultValue">The default value expression, if any.</param>
    /// <param name="ordinal">The one-based position of the parameter within the routine's signature.</param>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="direction"/> is not a valid enum value.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="ordinal"/> is less than one.</exception>
    public DatabaseRoutineParameter(
        Option<Identifier> parameterName,
        IDbType type,
        RoutineParameterDirection direction,
        Option<string> defaultValue,
        int ordinal
    )
    {
        if (!direction.IsValid())
            throw new ArgumentException($"The {nameof(RoutineParameterDirection)} provided must be a valid enum.", nameof(direction));
        ArgumentOutOfRangeException.ThrowIfLessThan(ordinal, 1);

        Name = parameterName.Map(static name => Identifier.CreateQualifiedIdentifier(name.LocalName));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Direction = direction;
        DefaultValue = defaultValue;
        Ordinal = ordinal;
    }

    /// <summary>
    /// The name of the parameter, when it has one.
    /// </summary>
    /// <value>A parameter name, or none when the parameter is positional.</value>
    public Option<Identifier> Name { get; }

    /// <summary>
    /// The type of data the parameter accepts.
    /// </summary>
    /// <value>A data type.</value>
    public IDbType Type { get; }

    /// <summary>
    /// The direction that values flow through the parameter.
    /// </summary>
    /// <value>A parameter direction.</value>
    public RoutineParameterDirection Direction { get; }

    /// <summary>
    /// The expression applied when no value is provided for the parameter, if any.
    /// </summary>
    /// <value>A default value expression.</value>
    public Option<string> DefaultValue { get; }

    /// <summary>
    /// The one-based position of the parameter within the routine's signature.
    /// </summary>
    /// <value>An ordinal position.</value>
    public int Ordinal { get; }

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

            builder.Append("Parameter: ")
                .Append(Name.Match(static name => name.LocalName, () => "$" + Ordinal.ToString(CultureInfo.InvariantCulture)))
                .Append(' ')
                .Append(Type.Definition);

            return builder.GetStringAndRelease();
        }
    }
}
