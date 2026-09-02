namespace SJP.Schematic.Core;

/// <summary>
/// Describes the kind of routine that a database object represents.
/// </summary>
public enum RoutineType
{
    /// <summary>
    /// The kind of the routine is not known, or the database does not record one.
    /// </summary>
    Unknown,

    /// <summary>
    /// A stored procedure, i.e. a routine that is invoked as a statement and returns no value.
    /// </summary>
    Procedure,

    /// <summary>
    /// A function, i.e. a routine that is invoked as an expression and returns a value.
    /// </summary>
    Function,

    /// <summary>
    /// A package, i.e. a named collection of routines declared and implemented together.
    /// Only Oracle exposes packages.
    /// </summary>
    Package,

    /// <summary>
    /// An aggregate function, i.e. a function that accumulates a result over a set of rows.
    /// </summary>
    Aggregate,
}
