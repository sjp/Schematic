using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A database key constraint.
/// </summary>
/// <seealso cref="IDatabaseKey" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseKey : IDatabaseKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseKey"/> class.
    /// </summary>
    /// <param name="name">A constraint name, if available. Only the local name is kept.</param>
    /// <param name="keyType">The key constraint type.</param>
    /// <param name="columns">The columns covered by the key.</param>
    /// <param name="isEnabled">Whether the constraint is enabled.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="keyType"/> is an invalid enum value.</exception>
    public DatabaseKey(Option<Identifier> name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled)
        : this(name, keyType, columns, isEnabled, Option<IDatabaseIndex>.None)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseKey"/> class.
    /// </summary>
    /// <param name="name">A constraint name, if available. Only the local name is kept.</param>
    /// <param name="keyType">The key constraint type.</param>
    /// <param name="columns">The columns covered by the key.</param>
    /// <param name="isEnabled">Whether the constraint is enabled.</param>
    /// <param name="backingIndex">The index used to enforce the constraint, if the database reports one.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="keyType"/> is an invalid enum value.</exception>
    public DatabaseKey(Option<Identifier> name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled, Option<IDatabaseIndex> backingIndex)
        : this(name, keyType, columns, isEnabled, backingIndex, true, ConstraintDeferrability.NotDeferrable)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseKey"/> class.
    /// </summary>
    /// <param name="name">A constraint name, if available. Only the local name is kept.</param>
    /// <param name="keyType">The key constraint type.</param>
    /// <param name="columns">The columns covered by the key.</param>
    /// <param name="isEnabled">Whether the constraint is enabled.</param>
    /// <param name="backingIndex">The index used to enforce the constraint, if the database reports one.</param>
    /// <param name="isValidated">Whether the existing rows are known to satisfy the constraint.</param>
    /// <param name="deferrability">When the database checks the constraint.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="keyType"/> or <paramref name="deferrability"/> is an invalid enum value.</exception>
    public DatabaseKey(
        Option<Identifier> name,
        DatabaseKeyType keyType,
        IReadOnlyCollection<IDatabaseColumn> columns,
        bool isEnabled,
        Option<IDatabaseIndex> backingIndex,
        bool isValidated,
        ConstraintDeferrability deferrability
    )
    {
        var keyColumns = columns.ToDefensiveCopy(nameof(columns));
        if (keyColumns.Empty())
            throw new ArgumentException("A key must have at least one column.", nameof(columns));
        if (!keyType.IsValid())
            throw new ArgumentException($"The {nameof(DatabaseKeyType)} provided must be a valid enum.", nameof(keyType));
        if (!deferrability.IsValid())
            throw new ArgumentException($"The {nameof(ConstraintDeferrability)} provided must be a valid enum.", nameof(deferrability));

        Name = name.Map(static n => Identifier.CreateQualifiedIdentifier(n.LocalName));
        KeyType = keyType;
        Columns = keyColumns;
        IsEnabled = isEnabled;
        BackingIndex = backingIndex;
        IsValidated = isValidated;
        Deferrability = deferrability;
    }

    /// <summary>
    /// The name of the key constraint, if available.
    /// </summary>
    /// <value>A constraint name, if available.</value>
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
    /// Indicates whether this database key is enabled.
    /// </summary>
    /// <value><see langword="true" /> if this object is enabled; otherwise, <see langword="false" />.</value>
    public bool IsEnabled { get; }

    /// <summary>
    /// Indicates whether the existing rows are known to satisfy the key constraint.
    /// </summary>
    /// <value><see langword="true" /> if the constraint has been validated; otherwise, <see langword="false" />.</value>
    public bool IsValidated { get; }

    /// <summary>
    /// Describes when the database checks the key constraint.
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