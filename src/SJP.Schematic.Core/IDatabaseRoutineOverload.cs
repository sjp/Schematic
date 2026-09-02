using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines one signature of a database routine that shares its name with other signatures.
/// </summary>
/// <seealso cref="IDatabaseRoutine.Overloads" />
public interface IDatabaseRoutineOverload
{
    /// <summary>
    /// The definition of this signature alone.
    /// </summary>
    /// <value>A textual routine definition.</value>
    string Definition { get; }

    /// <summary>
    /// The parameters this signature declares, ordered by <see cref="IDatabaseRoutineParameter.Ordinal"/>.
    /// </summary>
    /// <value>A collection of parameters.</value>
    IReadOnlyList<IDatabaseRoutineParameter> Parameters { get; }

    /// <summary>
    /// The type of value this signature returns, if it returns one.
    /// </summary>
    /// <value>A data type, or none for a procedure.</value>
    Option<IDbType> ReturnType { get; }
}
