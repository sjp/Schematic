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
    /// <param name="columns">The columns.</param>
    /// <param name="primaryKey">The primary key.</param>
    /// <param name="uniqueKeys">The unique keys.</param>
    /// <param name="parentKeys">The parent keys.</param>
    /// <param name="childKeys">The child keys.</param>
    /// <param name="indexes">The indexes.</param>
    /// <param name="checks">The checks.</param>
    /// <param name="triggers">The triggers.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="tableName"/> or <paramref name="columns"/> or <paramref name="uniqueKeys"/> or <paramref name="parentKeys"/> or <paramref name="childKeys"/> or <paramref name="indexes"/> or <paramref name="checks"/> or <paramref name="triggers"/>.
    /// If a given collection contains a <c>null</c> value, an <see cref="ArgumentNullException"/> will also be thrown.
    /// </exception>
    /// <exception cref="ArgumentException">When given key with mismatching key types, e.g. primary key or unique keys.</exception>
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
        if (columns.NullOrAnyNull())
            throw new ArgumentNullException(nameof(columns));
        if (uniqueKeys.NullOrAnyNull())
            throw new ArgumentNullException(nameof(uniqueKeys));
        if (parentKeys.NullOrAnyNull())
            throw new ArgumentNullException(nameof(parentKeys));
        if (childKeys.NullOrAnyNull())
            throw new ArgumentNullException(nameof(childKeys));
        if (indexes.NullOrAnyNull())
            throw new ArgumentNullException(nameof(indexes));
        if (checks.NullOrAnyNull())
            throw new ArgumentNullException(nameof(checks));
        if (triggers.NullOrAnyNull())
            throw new ArgumentNullException(nameof(triggers));

        primaryKey.IfSome(static pk =>
        {
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one
            if (pk.KeyType != DatabaseKeyType.Primary)
                throw new ArgumentException("The given primary key did not have a key type of '" + nameof(DatabaseKeyType.Primary) + "'", nameof(primaryKey));
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one
        });

        var anyNonUniqueKey = uniqueKeys.Any(static uk => uk.KeyType != DatabaseKeyType.Unique);
        if (anyNonUniqueKey)
            throw new ArgumentException("A given unique key did not have a key type of '" + nameof(DatabaseKeyType.Unique) + "'", nameof(uniqueKeys));

        Name = tableName ?? throw new ArgumentNullException(nameof(tableName));
        Columns = columns;
        PrimaryKey = primaryKey;
        UniqueKeys = uniqueKeys;
        ParentKeys = parentKeys;
        ChildKeys = childKeys;
        Indexes = indexes;
        Checks = checks;

        var act = new Activity("test");
        act.SetParentId("");

        Triggers = triggers;
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