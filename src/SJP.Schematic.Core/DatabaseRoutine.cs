using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// Contains information about database routines.
/// </summary>
/// <seealso cref="IDatabaseRoutine" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseRoutine : IDatabaseRoutine
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseRoutine"/> class, describing a routine
    /// whose kind, language and signature are not known.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="definition">The routine definition.</param>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/> or <paramref name="definition"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
    public DatabaseRoutine(Identifier routineName, string definition)
        : this(routineName, definition, RoutineType.Unknown, Option<string>.None, [], Option<IDbType>.None, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseRoutine"/> class, describing a routine
    /// that carries a single signature.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="definition">The routine definition.</param>
    /// <param name="routineType">The kind of routine.</param>
    /// <param name="language">The language the routine is written in, if known.</param>
    /// <param name="parameters">The parameters the routine declares.</param>
    /// <param name="returnType">The type of value the routine returns, if it returns one.</param>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/>, <paramref name="definition"/> or <paramref name="parameters"/> is <see langword="null" />, or <paramref name="parameters"/> contains a <see langword="null" /> element.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace, or <paramref name="routineType"/> is not a valid enum value.</exception>
    public DatabaseRoutine(
        Identifier routineName,
        string definition,
        RoutineType routineType,
        Option<string> language,
        IReadOnlyList<IDatabaseRoutineParameter> parameters,
        Option<IDbType> returnType
    )
        : this(routineName, definition, routineType, language, parameters, returnType, [])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseRoutine"/> class.
    /// </summary>
    /// <param name="routineName">A routine name.</param>
    /// <param name="definition">The routine definition.</param>
    /// <param name="routineType">The kind of routine.</param>
    /// <param name="language">The language the routine is written in, if known.</param>
    /// <param name="parameters">The parameters the routine declares. When <paramref name="overloads"/> is not empty these must be the first overload's parameters.</param>
    /// <param name="returnType">The type of value the routine returns, if it returns one.</param>
    /// <param name="overloads">Every signature declared under the routine's name, or empty when the name carries a single signature.</param>
    /// <exception cref="ArgumentNullException"><paramref name="routineName"/>, <paramref name="definition"/>, <paramref name="parameters"/> or <paramref name="overloads"/> is <see langword="null" />, or one of the collections contains a <see langword="null" /> element.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace, or <paramref name="routineType"/> is not a valid enum value.</exception>
    public DatabaseRoutine(
        Identifier routineName,
        string definition,
        RoutineType routineType,
        Option<string> language,
        IReadOnlyList<IDatabaseRoutineParameter> parameters,
        Option<IDbType> returnType,
        IReadOnlyList<IDatabaseRoutineOverload> overloads
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);
        if (!routineType.IsValid())
            throw new ArgumentException($"The {nameof(Core.RoutineType)} provided must be a valid enum.", nameof(routineType));
        if (parameters.NullOrAnyNull())
            throw new ArgumentNullException(nameof(parameters));
        if (overloads.NullOrAnyNull())
            throw new ArgumentNullException(nameof(overloads));

        Name = routineName ?? throw new ArgumentNullException(nameof(routineName));
        Definition = definition;
        RoutineType = routineType;
        Language = language;
        Parameters = parameters;
        ReturnType = returnType;
        Overloads = overloads;
    }

    /// <summary>
    /// The name of the database routine.
    /// </summary>
    public Identifier Name { get; }

    /// <summary>
    /// The definition of the routine.
    /// </summary>
    /// <value>A textual routine definition.</value>
    public string Definition { get; }

    /// <summary>
    /// The kind of routine, e.g. a procedure or a function.
    /// </summary>
    /// <value>A routine kind.</value>
    public RoutineType RoutineType { get; }

    /// <summary>
    /// The language the routine is written in, e.g. <c>SQL</c> or <c>plpgsql</c>.
    /// </summary>
    /// <value>A language name, or none when the database does not record one.</value>
    public Option<string> Language { get; }

    /// <summary>
    /// The parameters the routine declares, ordered by <see cref="IDatabaseRoutineParameter.Ordinal"/>.
    /// </summary>
    /// <value>A collection of parameters.</value>
    public IReadOnlyList<IDatabaseRoutineParameter> Parameters { get; }

    /// <summary>
    /// The type of value the routine returns, if it returns one.
    /// </summary>
    /// <value>A data type, or none for a procedure.</value>
    public Option<IDbType> ReturnType { get; }

    /// <summary>
    /// Every signature declared under this routine's name, when there is more than one.
    /// </summary>
    /// <value>A collection of signatures, empty when the name carries a single signature.</value>
    public IReadOnlyList<IDatabaseRoutineOverload> Overloads { get; }

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

            builder.Append("Routine: ");

            if (!Name.Schema.IsNullOrWhiteSpace())
                builder.Append(Name.Schema).Append('.');

            builder.Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}
