using System.ComponentModel;
using System.Diagnostics;

namespace SJP.Schematic.Core;

/// <summary>
/// A top-level object within a database schema: a table, view, sequence, synonym, routine or
/// user-defined type.
/// </summary>
/// <remarks>
/// <para>
/// Every kind of object has a <see cref="Name"/>, which is available directly. Switch on the object
/// to handle each kind in turn; the switch must name every kind, so code that does so stops
/// compiling when a new kind of object is added rather than falling through at runtime.
/// </para>
/// <para>
/// A materialized view is a view, so it is held as an <see cref="IDatabaseView"/>. A
/// <see langword="default"/> value holds no object, and every member throws when read from it.
/// </para>
/// </remarks>
/// <seealso cref="IDatabaseEntity" />
[DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
public readonly union DatabaseObject(
    IRelationalDatabaseTable,
    IDatabaseView,
    IDatabaseSequence,
    IDatabaseSynonym,
    IDatabaseRoutine,
    DatabaseUserDefinedType
) : IDatabaseEntity
{
    /// <summary>
    /// The name of the database object.
    /// </summary>
    /// <value>An object name.</value>
    public Identifier Name => this switch
    {
        IRelationalDatabaseTable table => table.Name,
        IDatabaseView view => view.Name,
        IDatabaseSequence sequence => sequence.Name,
        IDatabaseSynonym synonym => synonym.Name,
        IDatabaseRoutine routine => routine.Name,
        DatabaseUserDefinedType userDefinedType => userDefinedType.Name,
    };

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => Value?.ToString() ?? string.Empty;
}
