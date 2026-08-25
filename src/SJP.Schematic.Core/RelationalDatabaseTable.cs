using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A database table implementation, containing information about database tables.
/// </summary>
/// <seealso cref="IRelationalDatabaseTable" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class RelationalDatabaseTable : IRelationalDatabaseTable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationalDatabaseTable"/> class.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="columns">The columns. May be empty, as a provider is not always able to resolve the columns of a table, e.g. when the table is inaccessible.</param>
    /// <param name="primaryKey">The primary key.</param>
    /// <param name="uniqueKeys">The unique keys.</param>
    /// <param name="parentKeys">The parent keys.</param>
    /// <param name="childKeys">The child keys.</param>
    /// <param name="indexes">The indexes.</param>
    /// <param name="checks">The checks.</param>
    /// <param name="triggers">The triggers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="tableName"/>, <paramref name="columns"/>, <paramref name="uniqueKeys"/>, <paramref name="parentKeys"/>, <paramref name="childKeys"/>, <paramref name="indexes"/>, <paramref name="checks"/> or <paramref name="triggers"/> is <see langword="null" />, or one of the given collections contains <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="primaryKey"/> does not have a primary key type, or <paramref name="uniqueKeys"/> contains a key that does not have a unique key type.</exception>
    public RelationalDatabaseTable(
        Identifier tableName,
        IReadOnlyList<IDatabaseColumn> columns,
        Option<IDatabaseKey> primaryKey,
        IReadOnlyCollection<IDatabaseKey> uniqueKeys,
        IReadOnlyCollection<IDatabaseRelationalKey> parentKeys,
        IReadOnlyCollection<IDatabaseRelationalKey> childKeys,
        IReadOnlyCollection<IDatabaseIndex> indexes,
        IReadOnlyCollection<IDatabaseCheckConstraint> checks,
        IReadOnlyCollection<IDatabaseTrigger> triggers)
    {
        var tableColumns = columns.ToDefensiveCopy(nameof(columns));
        var tableUniqueKeys = uniqueKeys.ToDefensiveCopy(nameof(uniqueKeys));
        var tableParentKeys = parentKeys.ToDefensiveCopy(nameof(parentKeys));
        var tableChildKeys = childKeys.ToDefensiveCopy(nameof(childKeys));
        var tableIndexes = indexes.ToDefensiveCopy(nameof(indexes));
        var tableChecks = checks.ToDefensiveCopy(nameof(checks));
        var tableTriggers = triggers.ToDefensiveCopy(nameof(triggers));

        primaryKey.IfSome(static pk =>
        {
            if (pk.KeyType != DatabaseKeyType.Primary)
                throw new ArgumentException("The given primary key did not have a key type of '" + nameof(DatabaseKeyType.Primary) + "'", nameof(primaryKey));
        });

        var anyNonUniqueKey = tableUniqueKeys.Any(static uk => uk.KeyType != DatabaseKeyType.Unique);
        if (anyNonUniqueKey)
            throw new ArgumentException("A given unique key did not have a key type of '" + nameof(DatabaseKeyType.Unique) + "'", nameof(uniqueKeys));

        Name = tableName ?? throw new ArgumentNullException(nameof(tableName));
        Columns = tableColumns;
        PrimaryKey = primaryKey;
        UniqueKeys = tableUniqueKeys;
        ParentKeys = tableParentKeys;
        ChildKeys = tableChildKeys;
        Indexes = tableIndexes;
        Checks = tableChecks;
        Triggers = tableTriggers;
    }

    /// <summary>
    /// The table name.
    /// </summary>
    /// <value>The table name.</value>
    public Identifier Name { get; }

    /// <summary>
    /// The primary key of the table, if available.
    /// </summary>
    /// <value>A primary key, if available.</value>
    public Option<IDatabaseKey> PrimaryKey { get; }

    /// <summary>
    /// Indexes defined for the table.
    /// </summary>
    /// <value>A set of indexes.</value>
    public IReadOnlyCollection<IDatabaseIndex> Indexes { get; }

    /// <summary>
    /// Unique key constraints defined for the table.
    /// </summary>
    /// <value>Unique key constraints.</value>
    public IReadOnlyCollection<IDatabaseKey> UniqueKeys { get; }

    /// <summary>
    /// <para>A set of child foreign key constraints.</para>
    /// <para>Child keys form a relationship from a primary or unique key in the current table, to a foreign key constraint.</para>
    /// </summary>
    /// <value>The child keys.</value>
    /// <remarks>This is a convenient way of determining which records in a database may depend on a value defined in this table.</remarks>
    public IReadOnlyCollection<IDatabaseRelationalKey> ChildKeys { get; }

    /// <summary>
    /// Check constraints defined for the table.
    /// </summary>
    /// <value>Check constraints.</value>
    public IReadOnlyCollection<IDatabaseCheckConstraint> Checks { get; }

    /// <summary>
    /// <para>A set of parent foreign key constraints.</para>
    /// <para>Parent keys form a relationship from the current table's foreign key, to a unique or primary key.</para>
    /// </summary>
    /// <value>Foreign key constraints.</value>
    public IReadOnlyCollection<IDatabaseRelationalKey> ParentKeys { get; }

    /// <summary>
    /// The ordered list of columns in the table.
    /// </summary>
    /// <value>The table columns.</value>
    public IReadOnlyList<IDatabaseColumn> Columns { get; }

    /// <summary>
    /// The triggers defined on the table.
    /// </summary>
    /// <value>Triggers defined on the table.</value>
    public IReadOnlyCollection<IDatabaseTrigger> Triggers { get; }

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

            builder.Append("Table: ");

            if (!Name.Schema.IsNullOrWhiteSpace())
                builder.Append(Name.Schema).Append('.');

            builder.Append(Name.LocalName);

            return builder.GetStringAndRelease();
        }
    }
}