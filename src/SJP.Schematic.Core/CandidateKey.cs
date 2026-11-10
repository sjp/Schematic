using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// A key that a foreign key can refer to: a primary key or a unique key.
/// </summary>
/// <remarks>
/// A <see langword="default"/> value holds no key, and every member throws when read from it.
/// </remarks>
/// <seealso cref="DatabaseKey" />
/// <seealso cref="IDatabaseRelationalKey.ParentKey" />
[DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
public union CandidateKey(DatabaseKey.Primary, DatabaseKey.Unique)
{
    /// <summary>
    /// The name of the key constraint, if available.
    /// </summary>
    /// <value>A constraint name, if available.</value>
    public Option<Identifier> Name => this switch
    {
        DatabaseKey.Primary key => key.Name,
        DatabaseKey.Unique key => key.Name,
    };

    /// <summary>
    /// The columns that define the key constraint.
    /// </summary>
    /// <value>A collection of database columns.</value>
    public IReadOnlyCollection<IDatabaseColumn> Columns => this switch
    {
        DatabaseKey.Primary key => key.Columns,
        DatabaseKey.Unique key => key.Columns,
    };

    /// <summary>
    /// The index that the database uses to enforce the key constraint.
    /// </summary>
    /// <value>An index, if the database reports one for the constraint.</value>
    public Option<IDatabaseIndex> BackingIndex => this switch
    {
        DatabaseKey.Primary key => key.BackingIndex,
        DatabaseKey.Unique key => key.BackingIndex,
    };

    /// <summary>
    /// Indicates whether the key constraint is enabled.
    /// </summary>
    /// <value><see langword="true" /> if the constraint is enabled; otherwise, <see langword="false" />.</value>
    public bool IsEnabled => this switch
    {
        DatabaseKey.Primary key => key.IsEnabled,
        DatabaseKey.Unique key => key.IsEnabled,
    };

    /// <summary>
    /// Indicates whether the existing rows are known to satisfy the key constraint.
    /// </summary>
    /// <value><see langword="true" /> if the constraint has been validated; otherwise, <see langword="false" />.</value>
    public bool IsValidated => this switch
    {
        DatabaseKey.Primary key => key.IsValidated,
        DatabaseKey.Unique key => key.IsValidated,
    };

    /// <summary>
    /// Describes when the database checks the key constraint.
    /// </summary>
    /// <value>A deferrability value.</value>
    public ConstraintDeferrability Deferrability => this switch
    {
        DatabaseKey.Primary key => key.Deferrability,
        DatabaseKey.Unique key => key.Deferrability,
    };

    /// <summary>
    /// Widens a candidate key to a key of any kind.
    /// </summary>
    /// <param name="key">A primary or unique key.</param>
    public static implicit operator DatabaseKey(CandidateKey key) => key switch
    {
        DatabaseKey.Primary primaryKey => primaryKey,
        DatabaseKey.Unique uniqueKey => uniqueKey,
    };

    /// <summary>
    /// Widens a candidate key to a key of any kind.
    /// </summary>
    /// <returns>The same key, as a <see cref="DatabaseKey"/>.</returns>
    public DatabaseKey ToDatabaseKey() => this;

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => Value?.ToString() ?? string.Empty;
}
