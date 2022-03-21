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
}