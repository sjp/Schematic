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
    /// <param name="isEnabled">if set to <c>true</c> [is enabled].</param>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <c>null</c>. Alternatively if <paramref name="columns"/> is <c>null</c>, empty or contains a <c>null</c> value.</exception>
    /// <exception cref="ArgumentException"><paramref name="keyType"/> is an invalid enum value.</exception>
    public SqlServerDatabaseKey(Identifier name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        if (columns == null || columns.Empty() || columns.AnyNull())
            throw new ArgumentNullException(nameof(columns));
        if (!keyType.IsValid())
            throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

        Name = Option<Identifier>.Some(name.LocalName);
        KeyType = keyType;
        Columns = columns;
        IsEnabled = isEnabled;
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
    /// <value><c>true</c> if this key constraint is enabled; otherwise, <c>false</c>.</value>
    public bool IsEnabled { get; }

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