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
    /// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="columns"/> is <c>null</c>. Alternatively, if <paramref name="columns"/> is empty or has <c>null</c> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="keyType"/> is not a valid enum.</exception>
    public SqliteDatabaseKey(Option<Identifier> name, DatabaseKeyType keyType, IEnumerable<IDatabaseColumn> columns)
    {
        if (columns.NullOrEmpty() || columns.AnyNull())
            throw new ArgumentNullException(nameof(columns));
        if (!keyType.IsValid())
            throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

        Name = name.Map(static n => Identifier.CreateQualifiedIdentifier(n.LocalName));
        KeyType = keyType;
        Columns = columns.ToList();
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
    /// Indicates whether this instance is enabled. Always <c>true</c>.
    /// </summary>
    /// <value>Always <c>true</c>.</value>
    public bool IsEnabled { get; } = true;

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