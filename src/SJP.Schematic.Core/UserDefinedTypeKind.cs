namespace SJP.Schematic.Core;

/// <summary>
/// Describes the kind of a user-defined type.
/// </summary>
public enum UserDefinedTypeKind
{
    /// <summary>
    /// The kind of the type could not be determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// An alias for a built-in type, e.g. a SQL Server type created by <c>create type ... from</c>.
    /// </summary>
    Alias,

    /// <summary>
    /// A type defined as a built-in type constrained by check constraints, a nullability rule and
    /// a default, e.g. a PostgreSQL domain.
    /// </summary>
    Domain,

    /// <summary>
    /// A type whose values are restricted to a named set of labels, e.g. a PostgreSQL enum.
    /// </summary>
    Enum,

    /// <summary>
    /// A type composed of named attributes, e.g. a PostgreSQL composite type or an Oracle object type.
    /// </summary>
    Composite,

    /// <summary>
    /// A type describing a range of values of a subtype, e.g. a PostgreSQL range type.
    /// </summary>
    Range,

    /// <summary>
    /// A type describing a table, e.g. a SQL Server table type used for table-valued parameters.
    /// </summary>
    Table,

    /// <summary>
    /// A type implemented by managed code hosted in the database, e.g. a SQL Server assembly type.
    /// </summary>
    Clr,

    /// <summary>
    /// A type describing a collection of values of an element type, e.g. an Oracle varray or nested table.
    /// </summary>
    Collection,
}
