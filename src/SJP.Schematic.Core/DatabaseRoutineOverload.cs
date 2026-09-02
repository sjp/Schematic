using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core;

/// <summary>
/// One signature of a database routine that shares its name with other signatures.
/// </summary>
/// <seealso cref="IDatabaseRoutineOverload" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseRoutineOverload : IDatabaseRoutineOverload
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseRoutineOverload"/> class.
    /// </summary>
    /// <param name="definition">The definition of this signature alone.</param>
    /// <param name="parameters">The parameters this signature declares.</param>
    /// <param name="returnType">The type of value this signature returns, if it returns one.</param>
    /// <exception cref="ArgumentNullException"><paramref name="definition"/> or <paramref name="parameters"/> is <see langword="null" />, or <paramref name="parameters"/> contains a <see langword="null" /> element.</exception>
    /// <exception cref="ArgumentException"><paramref name="definition"/> is empty or whitespace.</exception>
    public DatabaseRoutineOverload(string definition, IReadOnlyList<IDatabaseRoutineParameter> parameters, Option<IDbType> returnType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(definition);
        if (parameters.NullOrAnyNull())
            throw new ArgumentNullException(nameof(parameters));

        Definition = definition;
        Parameters = parameters;
        ReturnType = returnType;
    }

    /// <summary>
    /// The definition of this signature alone.
    /// </summary>
    /// <value>A textual routine definition.</value>
    public string Definition { get; }

    /// <summary>
    /// The parameters this signature declares, ordered by <see cref="IDatabaseRoutineParameter.Ordinal"/>.
    /// </summary>
    /// <value>A collection of parameters.</value>
    public IReadOnlyList<IDatabaseRoutineParameter> Parameters { get; }

    /// <summary>
    /// The type of value this signature returns, if it returns one.
    /// </summary>
    /// <value>A data type, or none for a procedure.</value>
    public Option<IDbType> ReturnType { get; }

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay => "Overload: (" + Parameters.Select(static p => p.Type.Definition).Join(", ") + ")";
}
