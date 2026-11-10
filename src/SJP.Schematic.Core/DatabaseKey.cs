﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A key constraint on a table: a primary key, a unique key or a foreign key.
/// </summary>
/// <remarks>
/// <para>
/// The members that every kind of key has are available directly. Switch on the key to reach the
/// members that only some kinds have, such as the <see cref="Primary.BackingIndex"/> that only
/// primary and unique keys carry.
/// </para>
/// <para>
/// A <see langword="default"/> value holds no key, and every member throws when read from it.
/// </para>
/// </remarks>
/// <seealso cref="CandidateKey" />
[DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
public union DatabaseKey(DatabaseKey.Primary, DatabaseKey.Unique, DatabaseKey.Foreign)
{
    /// <summary>
    /// The name of the key constraint, if available.
    /// </summary>
    /// <value>A constraint name, if available.</value>
    public Option<Identifier> Name => this switch
    {
        Primary key => key.Name,
        Unique key => key.Name,
        Foreign key => key.Name,
    };

    /// <summary>
    /// The columns that define the key constraint.
    /// </summary>
    /// <value>A collection of database columns.</value>
    public IReadOnlyCollection<IDatabaseColumn> Columns => this switch
    {
        Primary key => key.Columns,
        Unique key => key.Columns,
        Foreign key => key.Columns,
    };

    /// <summary>
    /// Indicates whether the key constraint is enabled.
    /// </summary>
    /// <value><see langword="true" /> if the constraint is enabled; otherwise, <see langword="false" />.</value>
    public bool IsEnabled => this switch
    {
        Primary key => key.IsEnabled,
        Unique key => key.IsEnabled,
        Foreign key => key.IsEnabled,
    };

    /// <summary>
    /// Indicates whether the existing rows are known to satisfy the key constraint.
    /// </summary>
    /// <value><see langword="true" /> if the constraint has been validated; otherwise, <see langword="false" />.</value>
    public bool IsValidated => this switch
    {
        Primary key => key.IsValidated,
        Unique key => key.IsValidated,
        Foreign key => key.IsValidated,
    };

    /// <summary>
    /// Describes when the database checks the key constraint.
    /// </summary>
    /// <value>A deferrability value.</value>
    public ConstraintDeferrability Deferrability => this switch
    {
        Primary key => key.Deferrability,
        Unique key => key.Deferrability,
        Foreign key => key.Deferrability,
    };

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => Value?.ToString() ?? string.Empty;

    /// <summary>
    /// A primary key constraint.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Primary : IDatabaseConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Primary"/> class.
        /// </summary>
        /// <param name="name">A constraint name, if available. Only the local name is kept.</param>
        /// <param name="columns">The columns covered by the key.</param>
        /// <param name="isEnabled">Whether the constraint is enabled.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
        /// <exception cref="ArgumentException"><paramref name="columns"/> is empty.</exception>
        public Primary(Option<Identifier> name, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled)
            : this(name, columns, isEnabled, Option<IDatabaseIndex>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Primary"/> class.
        /// </summary>
        /// <param name="name">A constraint name, if available. Only the local name is kept.</param>
        /// <param name="columns">The columns covered by the key.</param>
        /// <param name="isEnabled">Whether the constraint is enabled.</param>
        /// <param name="backingIndex">The index used to enforce the constraint, if the database reports one.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
        /// <exception cref="ArgumentException"><paramref name="columns"/> is empty.</exception>
        public Primary(Option<Identifier> name, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled, Option<IDatabaseIndex> backingIndex)
            : this(name, columns, isEnabled, backingIndex, true, ConstraintDeferrability.NotDeferrable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Primary"/> class.
        /// </summary>
        /// <param name="name">A constraint name, if available. Only the local name is kept.</param>
        /// <param name="columns">The columns covered by the key.</param>
        /// <param name="isEnabled">Whether the constraint is enabled.</param>
        /// <param name="backingIndex">The index used to enforce the constraint, if the database reports one.</param>
        /// <param name="isValidated">Whether the existing rows are known to satisfy the constraint.</param>
        /// <param name="deferrability">When the database checks the constraint.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
        /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="deferrability"/> is an invalid enum value.</exception>
        public Primary(
            Option<Identifier> name,
            IReadOnlyCollection<IDatabaseColumn> columns,
            bool isEnabled,
            Option<IDatabaseIndex> backingIndex,
            bool isValidated,
            ConstraintDeferrability deferrability
        )
        {
            Columns = KeyArguments.CopyColumns(columns);
            Deferrability = KeyArguments.ValidateDeferrability(deferrability);
            Name = KeyArguments.LocalName(name);
            IsEnabled = isEnabled;
            BackingIndex = backingIndex;
            IsValidated = isValidated;
        }

        /// <summary>
        /// The name of the key constraint, if available.
        /// </summary>
        /// <value>A constraint name, if available.</value>
        public Option<Identifier> Name { get; }

        /// <summary>
        /// The columns that define the key constraint.
        /// </summary>
        /// <value>A collection of database columns.</value>
        public IReadOnlyCollection<IDatabaseColumn> Columns { get; }

        /// <summary>
        /// The index that the database uses to enforce the key constraint.
        /// </summary>
        /// <remarks>
        /// The backing index is not repeated in <see cref="IRelationalDatabaseTable.Indexes"/>.
        /// </remarks>
        /// <value>An index, if the database reports one for the constraint.</value>
        public Option<IDatabaseIndex> BackingIndex { get; }

        /// <summary>
        /// Indicates whether the key constraint is enabled.
        /// </summary>
        /// <value><see langword="true" /> if the constraint is enabled; otherwise, <see langword="false" />.</value>
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
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => KeyArguments.Describe("Primary", Name);
    }

    /// <summary>
    /// A unique key constraint.
    /// </summary>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Unique : IDatabaseConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Unique"/> class.
        /// </summary>
        /// <param name="name">A constraint name, if available. Only the local name is kept.</param>
        /// <param name="columns">The columns covered by the key.</param>
        /// <param name="isEnabled">Whether the constraint is enabled.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
        /// <exception cref="ArgumentException"><paramref name="columns"/> is empty.</exception>
        public Unique(Option<Identifier> name, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled)
            : this(name, columns, isEnabled, Option<IDatabaseIndex>.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Unique"/> class.
        /// </summary>
        /// <param name="name">A constraint name, if available. Only the local name is kept.</param>
        /// <param name="columns">The columns covered by the key.</param>
        /// <param name="isEnabled">Whether the constraint is enabled.</param>
        /// <param name="backingIndex">The index used to enforce the constraint, if the database reports one.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
        /// <exception cref="ArgumentException"><paramref name="columns"/> is empty.</exception>
        public Unique(Option<Identifier> name, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled, Option<IDatabaseIndex> backingIndex)
            : this(name, columns, isEnabled, backingIndex, true, ConstraintDeferrability.NotDeferrable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Unique"/> class.
        /// </summary>
        /// <param name="name">A constraint name, if available. Only the local name is kept.</param>
        /// <param name="columns">The columns covered by the key.</param>
        /// <param name="isEnabled">Whether the constraint is enabled.</param>
        /// <param name="backingIndex">The index used to enforce the constraint, if the database reports one.</param>
        /// <param name="isValidated">Whether the existing rows are known to satisfy the constraint.</param>
        /// <param name="deferrability">When the database checks the constraint.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
        /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="deferrability"/> is an invalid enum value.</exception>
        public Unique(
            Option<Identifier> name,
            IReadOnlyCollection<IDatabaseColumn> columns,
            bool isEnabled,
            Option<IDatabaseIndex> backingIndex,
            bool isValidated,
            ConstraintDeferrability deferrability
        )
        {
            Columns = KeyArguments.CopyColumns(columns);
            Deferrability = KeyArguments.ValidateDeferrability(deferrability);
            Name = KeyArguments.LocalName(name);
            IsEnabled = isEnabled;
            BackingIndex = backingIndex;
            IsValidated = isValidated;
        }

        /// <summary>
        /// The name of the key constraint, if available.
        /// </summary>
        /// <value>A constraint name, if available.</value>
        public Option<Identifier> Name { get; }

        /// <summary>
        /// The columns that define the key constraint.
        /// </summary>
        /// <value>A collection of database columns.</value>
        public IReadOnlyCollection<IDatabaseColumn> Columns { get; }

        /// <summary>
        /// The index that the database uses to enforce the key constraint.
        /// </summary>
        /// <remarks>
        /// The backing index is not repeated in <see cref="IRelationalDatabaseTable.Indexes"/>.
        /// </remarks>
        /// <value>An index, if the database reports one for the constraint.</value>
        public Option<IDatabaseIndex> BackingIndex { get; }

        /// <summary>
        /// Indicates whether the key constraint is enabled.
        /// </summary>
        /// <value><see langword="true" /> if the constraint is enabled; otherwise, <see langword="false" />.</value>
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
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => KeyArguments.Describe("Unique", Name);
    }

    /// <summary>
    /// A foreign key constraint, as declared on the table that holds it.
    /// </summary>
    /// <remarks>
    /// A foreign key is not enforced by an index of its own, so unlike a primary or unique key it has
    /// no backing index. The key it refers to is described by <see cref="IDatabaseRelationalKey"/>.
    /// </remarks>
    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public sealed class Foreign : IDatabaseConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Foreign"/> class.
        /// </summary>
        /// <param name="name">A constraint name, if available. Only the local name is kept.</param>
        /// <param name="columns">The columns covered by the key.</param>
        /// <param name="isEnabled">Whether the constraint is enabled.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
        /// <exception cref="ArgumentException"><paramref name="columns"/> is empty.</exception>
        public Foreign(Option<Identifier> name, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled)
            : this(name, columns, isEnabled, true, ConstraintDeferrability.NotDeferrable)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Foreign"/> class.
        /// </summary>
        /// <param name="name">A constraint name, if available. Only the local name is kept.</param>
        /// <param name="columns">The columns covered by the key.</param>
        /// <param name="isEnabled">Whether the constraint is enabled.</param>
        /// <param name="isValidated">Whether the existing rows are known to satisfy the constraint.</param>
        /// <param name="deferrability">When the database checks the constraint.</param>
        /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <see langword="null" /> or contains <see langword="null" /> values.</exception>
        /// <exception cref="ArgumentException"><paramref name="columns"/> is empty, or <paramref name="deferrability"/> is an invalid enum value.</exception>
        public Foreign(
            Option<Identifier> name,
            IReadOnlyCollection<IDatabaseColumn> columns,
            bool isEnabled,
            bool isValidated,
            ConstraintDeferrability deferrability
        )
        {
            Columns = KeyArguments.CopyColumns(columns);
            Deferrability = KeyArguments.ValidateDeferrability(deferrability);
            Name = KeyArguments.LocalName(name);
            IsEnabled = isEnabled;
            IsValidated = isValidated;
        }

        /// <summary>
        /// The name of the key constraint, if available.
        /// </summary>
        /// <value>A constraint name, if available.</value>
        public Option<Identifier> Name { get; }

        /// <summary>
        /// The columns that define the key constraint.
        /// </summary>
        /// <value>A collection of database columns.</value>
        public IReadOnlyCollection<IDatabaseColumn> Columns { get; }

        /// <summary>
        /// Indicates whether the key constraint is enabled.
        /// </summary>
        /// <value><see langword="true" /> if the constraint is enabled; otherwise, <see langword="false" />.</value>
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
        /// Returns a string that provides a basic string representation of this object.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => KeyArguments.Describe("Foreign", Name);
    }
}

/// <summary>
/// Argument handling shared by the key constraint cases.
/// </summary>
file static class KeyArguments
{
    public static IDatabaseColumn[] CopyColumns(IReadOnlyCollection<IDatabaseColumn> columns)
    {
        var keyColumns = columns.ToDefensiveCopy(nameof(columns));
        if (keyColumns.Empty())
            throw new ArgumentException("A key must have at least one column.", nameof(columns));

        return keyColumns;
    }

    public static ConstraintDeferrability ValidateDeferrability(ConstraintDeferrability deferrability)
    {
        if (!deferrability.IsValid())
            throw new ArgumentException($"The {nameof(ConstraintDeferrability)} provided must be a valid enum.", nameof(deferrability));

        return deferrability;
    }

    public static Option<Identifier> LocalName(Option<Identifier> name) => name.Map(static n => LocalIdentifier.From(n));

    public static string Describe(string keyKind, Option<Identifier> name)
    {
        var builder = StringBuilderCache.Acquire();

        builder.Append(keyKind)
            .Append(" Key");

        name.IfSome(n =>
        {
            builder.Append(": ")
                .Append(n.LocalName);
        });

        return builder.GetStringAndRelease();
    }
}
