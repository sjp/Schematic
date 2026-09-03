using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Sqlite;

/// <summary>
/// A database key implementation, specific to SQLite.
/// </summary>
/// <seealso cref="IDatabaseKey" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class SqliteDatabaseKey : IDatabaseKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDatabaseKey"/> class.
    /// </summary>
    /// <param name="name">The constraint name, if available.</param>
    /// <param name="keyType">Type of the key constraint.</param>
    /// <param name="columns">A collection of table columns.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or has <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="keyType"/> is not a valid enum.</exception>
    public SqliteDatabaseKey(Option<Identifier> name, DatabaseKeyType keyType, IEnumerable<IDatabaseColumn> columns)
        : this(name, keyType, columns, Option<IDatabaseIndex>.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDatabaseKey"/> class.
    /// </summary>
    /// <param name="name">The constraint name, if available.</param>
    /// <param name="keyType">Type of the key constraint.</param>
    /// <param name="columns">A collection of table columns.</param>
    /// <param name="backingIndex">The index used to enforce the constraint, if the database reports one.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or has <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="keyType"/> is not a valid enum.</exception>
    public SqliteDatabaseKey(Option<Identifier> name, DatabaseKeyType keyType, IEnumerable<IDatabaseColumn> columns, Option<IDatabaseIndex> backingIndex)
        : this(name, keyType, columns, backingIndex, ConstraintDeferrability.NotDeferrable)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteDatabaseKey"/> class.
    /// </summary>
    /// <param name="name">A constraint name, if available.</param>
    /// <param name="keyType">The key constraint type.</param>
    /// <param name="columns">The columns covered by the key.</param>
    /// <param name="backingIndex">The index used to enforce the constraint, if the database reports one.</param>
    /// <param name="deferrability">The <c>DEFERRABLE</c> behaviour declared in the table's definition.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="keyType"/> or <paramref name="deferrability"/> is an invalid enum value.</exception>
    public SqliteDatabaseKey(Option<Identifier> name, DatabaseKeyType keyType, IEnumerable<IDatabaseColumn> columns, Option<IDatabaseIndex> backingIndex, ConstraintDeferrability deferrability)
    {
        if (columns.NullOrAnyNull())
            throw new ArgumentNullException(nameof(columns));
        if (columns.Empty())
            throw new ArgumentException("A key must have at least one column.", nameof(columns));
        if (!keyType.IsValid())
            throw new ArgumentException($"The {nameof(DatabaseKeyType)} provided must be a valid enum.", nameof(keyType));
        if (!deferrability.IsValid())
            throw new ArgumentException($"The {nameof(ConstraintDeferrability)} provided must be a valid enum.", nameof(deferrability));

        Name = name.Map(static n => Identifier.CreateQualifiedIdentifier(n.LocalName));
        KeyType = keyType;
        Columns = columns.ToList();
        BackingIndex = backingIndex;
        Deferrability = deferrability;
    }

    /// <summary>
    /// The name of the key constraint.
    /// </summary>
    /// <value>A constraint name.</value>
    public Option<Identifier> Name { get; }

    /// <summary>
    /// The type of key constraint, e.g. primary, unique, foreign.
    /// </summary>
    /// <value>A key constraint type.</value>
    public DatabaseKeyType KeyType { get; }

    /// <summary>
    /// The columns that defines the key constraint.
    /// </summary>
    /// <value>A collection of database columns.</value>
    public IReadOnlyCollection<IDatabaseColumn> Columns { get; }

    /// <summary>
    /// Indicates whether this instance is enabled. Always <see langword="true" />.
    /// </summary>
    /// <value>Always <see langword="true" />.</value>
    public bool IsEnabled { get; } = true;

    /// <summary>
    /// Indicates whether the existing rows have been verified against the key constraint. Always
    /// <see langword="true" />; SQLite has no unvalidated key constraints.
    /// </summary>
    /// <value>Always <see langword="true" />.</value>
    public bool IsValidated { get; } = true;

    /// <summary>
    /// Describes when SQLite checks the key constraint. Only a foreign key can be deferred; primary
    /// and unique keys are always <see cref="ConstraintDeferrability.NotDeferrable"/>.
    /// </summary>
    /// <value>A deferrability value.</value>
    public ConstraintDeferrability Deferrability { get; }

    /// <summary>
    /// The index that the database uses to enforce the key constraint.
    /// </summary>
    /// <value>An index, if the database reports one for the constraint.</value>
    public Option<IDatabaseIndex> BackingIndex { get; }

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

            builder.Append(KeyType.ToString())
                .Append(" Key");

            Name.IfSome(name =>
            {
                builder.Append(": ")
                    .Append(name.LocalName);
            });

            return builder.GetStringAndRelease();
        }
    }
}