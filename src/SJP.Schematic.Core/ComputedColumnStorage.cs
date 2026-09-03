namespace SJP.Schematic.Core;

/// <summary>
/// Describes whether a database stores the values of a computed column, or evaluates its
/// expression whenever the column is read.
/// </summary>
public enum ComputedColumnStorage
{
    /// <summary>
    /// The storage of the computed column is not known, either because the column is not computed,
    /// or because the database does not report how the values are kept.
    /// </summary>
    Unknown,

    /// <summary>
    /// The expression is evaluated whenever the column is read, and no value is kept in the table,
    /// i.e. SQL Server non-persisted computed columns, Oracle virtual columns, MySQL
    /// <c>VIRTUAL GENERATED</c> columns and SQLite <c>GENERATED ALWAYS AS ... VIRTUAL</c> columns.
    /// </summary>
    Virtual,

    /// <summary>
    /// The value is computed when a row is written and kept in the table, i.e. SQL Server persisted
    /// computed columns, PostgreSQL <c>GENERATED ALWAYS AS ... STORED</c> columns, MySQL
    /// <c>STORED GENERATED</c> columns and SQLite <c>GENERATED ALWAYS AS ... STORED</c> columns.
    /// </summary>
    Stored,
}
