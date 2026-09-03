using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.SqlServer;

/// <summary>
/// A database key implementation, specific to SQL Server.
/// </summary>
/// <seealso cref="IDatabaseKey" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class SqlServerDatabaseKey : IDatabaseKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseKey"/> class.
    /// </summary>
    /// <param name="name">A key constraint name.</param>
    /// <param name="keyType">The key type.</param>
    /// <param name="columns">The columns comprised by the key.</param>
    /// <param name="isEnabled">if set to <see langword="true" /> [is enabled].</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="columns"/> is <see langword="null" />, or <paramref name="columns"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="keyType"/> is an invalid enum value.</exception>
    public SqlServerDatabaseKey(Identifier name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled)
        : this(name, keyType, columns, isEnabled, Option<IDatabaseIndex>.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseKey"/> class.
    /// </summary>
    /// <param name="name">A key constraint name.</param>
    /// <param name="keyType">The key type.</param>
    /// <param name="columns">The columns comprised by the key.</param>
    /// <param name="isEnabled">if set to <see langword="true" /> [is enabled].</param>
    /// <param name="backingIndex">The index used to enforce the constraint, if the database reports one.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="columns"/> is <see langword="null" />, or <paramref name="columns"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="keyType"/> is an invalid enum value.</exception>
    public SqlServerDatabaseKey(Identifier name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled, Option<IDatabaseIndex> backingIndex)
        : this(name, keyType, columns, isEnabled, backingIndex, true)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDatabaseKey"/> class.
    /// </summary>
    /// <param name="name">A key constraint name.</param>
    /// <param name="keyType">The key type.</param>
    /// <param name="columns">The columns comprised by the key.</param>
    /// <param name="isEnabled">if set to <see langword="true" /> [is enabled].</param>
    /// <param name="backingIndex">The index used to enforce the constraint, if the database reports one.</param>
    /// <param name="isValidated">Whether SQL Server trusts the constraint, i.e. <c>is_not_trusted</c> is not set.</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="columns"/> is <see langword="null" />, or <paramref name="columns"/> contains a <see langword="null" /> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="keyType"/> is an invalid enum value.</exception>
    public SqlServerDatabaseKey(Identifier name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled, Option<IDatabaseIndex> backingIndex, bool isValidated)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (columns.NullOrAnyNull())
            throw new ArgumentNullException(nameof(columns));
        if (columns.Empty())
            throw new ArgumentException("A key must have at least one column.", nameof(columns));
        if (!keyType.IsValid())
            throw new ArgumentException($"The {nameof(DatabaseKeyType)} provided must be a valid enum.", nameof(keyType));

        Name = Option<Identifier>.Some(name.LocalName);
        KeyType = keyType;
        Columns = columns;
        IsEnabled = isEnabled;
        BackingIndex = backingIndex;
        IsValidated = isValidated;
    }

    /// <summary>
    /// The name of the key constraint.
    /// </summary>
    /// <value>A constraint name.
    /// </value>
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
    /// Indicates whether this key constraint is enabled.
    /// </summary>
    /// <value><see langword="true" /> if this key constraint is enabled; otherwise, <see langword="false" />.</value>
    public bool IsEnabled { get; }

    /// <summary>
    /// Indicates whether SQL Server trusts the key constraint, i.e. the existing rows have been
    /// verified against it. Only a foreign key created or re-enabled <c>WITH NOCHECK</c> is untrusted.
    /// </summary>
    /// <value><see langword="true" /> if the constraint is trusted; otherwise, <see langword="false" />.</value>
    public bool IsValidated { get; }

    /// <summary>
    /// Always <see cref="ConstraintDeferrability.NotDeferrable"/>; SQL Server has no deferrable constraints.
    /// </summary>
    /// <value><see cref="ConstraintDeferrability.NotDeferrable"/>.</value>
    public ConstraintDeferrability Deferrability { get; } = ConstraintDeferrability.NotDeferrable;

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