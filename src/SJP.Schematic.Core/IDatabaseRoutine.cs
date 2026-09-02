using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a database routine.
/// </summary>
/// <seealso cref="IDatabaseEntity" />
public interface IDatabaseRoutine : IDatabaseEntity
{
    /// <summary>
    /// The definition of the routine.
    /// </summary>
    /// <value>A textual routine definition.</value>
    string Definition { get; }

    /// <summary>
    /// The kind of routine, e.g. a procedure or a function.
    /// </summary>
    /// <value>A routine kind, or <see cref="Core.RoutineType.Unknown"/> when the database does not record one.</value>
    RoutineType RoutineType { get; }

    /// <summary>
    /// The language the routine is written in, e.g. <c>SQL</c> or <c>plpgsql</c>.
    /// </summary>
    /// <value>A language name, or none when the database does not record one.</value>
    Option<string> Language { get; }

    /// <summary>
    /// The parameters the routine declares, ordered by <see cref="IDatabaseRoutineParameter.Ordinal"/>.
    /// </summary>
    /// <value>
    /// A collection of parameters. Empty when the routine takes none, and also when the database
    /// does not describe the routine's parameters. When <see cref="Overloads"/> is not empty these
    /// are the first overload's parameters.
    /// </value>
    IReadOnlyList<IDatabaseRoutineParameter> Parameters { get; }

    /// <summary>
    /// The type of value the routine returns, if it returns one.
    /// </summary>
    /// <value>
    /// A data type, or none for a procedure and for a routine whose return the database does not
    /// describe. When <see cref="Overloads"/> is not empty this is the first overload's return type.
    /// </value>
    Option<IDbType> ReturnType { get; }

    /// <summary>
    /// Every signature declared under this routine's name, when there is more than one.
    /// </summary>
    /// <value>
    /// <para>
    /// A collection of signatures, empty for the ordinary case of a name that carries a single
    /// signature — <see cref="Parameters"/> and <see cref="ReturnType"/> describe that one.
    /// </para>
    /// <para>
    /// PostgreSQL is the only supported database that permits overloading, so this is empty
    /// everywhere else. When it is populated it holds every signature, including the one described
    /// by <see cref="Parameters"/> and <see cref="ReturnType"/>, and <see cref="Definition"/> is
    /// the definitions of all of them in the same order.
    /// </para>
    /// </value>
    IReadOnlyList<IDatabaseRoutineOverload> Overloads { get; }
}
