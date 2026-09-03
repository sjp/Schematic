using System.Collections.Generic;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Defines a relational database table object.
/// </summary>
/// <seealso cref="IDatabaseQueryable" />
public interface IRelationalDatabaseTable : IDatabaseQueryable
{
    /// <summary>
    /// The primary key of the table, if available.
    /// </summary>
    /// <value>A primary key, if available.</value>
    Option<IDatabaseKey> PrimaryKey { get; }

    /// <summary>
    /// The ordered list of columns in the table.
    /// </summary>
    /// <remarks>
    /// A column the user declared but the engine omits from <c>SELECT *</c> is listed, and reports
    /// <see cref="IDatabaseColumn.IsHidden"/> as <see langword="true" />; it is still a column of the
    /// table, and can be named by a key or an <c>INSERT</c>. Columns the engine created for its own
    /// use, such as the ones Oracle stores behind a function-based index or SQLite keeps in the
    /// shadow tables of a virtual table, are not listed at all.
    /// </remarks>
    /// <value>The table columns.</value>
    IReadOnlyList<IDatabaseColumn> Columns { get; }

    /// <summary>
    /// Check constraints defined for the table.
    /// </summary>
    /// <value>Check constraints.</value>
    IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

    /// <summary>
    /// Indexes defined for the table.
    /// </summary>
    /// <remarks>
    /// Only indexes created in their own right are listed. An index that exists solely to enforce a
    /// primary or unique key constraint is reported by <see cref="IDatabaseKey.BackingIndex"/> on that
    /// constraint instead, so that a constraint and its index are never counted twice.
    /// </remarks>
    /// <value>A set of indexes.</value>
    IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

    /// <summary>
    /// Unique key constraints defined for the table.
    /// </summary>
    /// <value>Unique key constraints.</value>
    IReadOnlyCollection<IDatabaseKey> UniqueKeys { get; }

    /// <summary>
    /// <para>A set of parent foreign key constraints.</para>
    /// <para>Parent keys form a relationship from the current table's foreign key, to a unique or primary key.</para>
    /// </summary>
    /// <value>Foreign key constraints.</value>
    IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys { get; }

    /// <summary>
    /// <para>A set of child foreign key constraints.</para>
    /// <para>Child keys form a relationship from a primary or unique key in the current table, to a foreign key constraint.</para>
    /// </summary>
    /// <value>
    /// The child keys.
    /// </value>
    /// <remarks>This is a convenient way of determining which records in a database may depend on a value defined in this table.</remarks>
    IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys { get; }

    /// <summary>
    /// The triggers defined on the table.
    /// </summary>
    /// <value>Triggers defined on the table.</value>
    IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }

    /// <summary>
    /// What the table is, where that differs from an ordinary persistent table.
    /// </summary>
    /// <value>A table kind.</value>
    TableKind Kind { get; }

    /// <summary>
    /// How the table's rows are distributed across partitions, if it is partitioned.
    /// </summary>
    /// <value>Partitioning information, if the table is partitioned.</value>
    Option<ITablePartitioning> Partitioning { get; }

    /// <summary>
    /// Where the table's superseded rows are retained, if the table is system-versioned.
    /// </summary>
    /// <value>System versioning information, if the table is system-versioned.</value>
    Option<ITableSystemVersioning> SystemVersioning { get; }

    /// <summary>
    /// Whether writes to the table are written to the database's transaction log.
    /// </summary>
    /// <value>
    /// <see langword="false" /> for storage that trades durability for speed, e.g. a PostgreSQL
    /// unlogged table or a SQL Server memory-optimized table with <c>SCHEMA_ONLY</c> durability;
    /// otherwise <see langword="true" />.
    /// </value>
    bool IsLogged { get; }

    /// <summary>
    /// The default collation applied to the table's character data.
    /// </summary>
    /// <value>A collation, if the database records one for the table as a whole.</value>
    Option<Identifier> Collation { get; }
}