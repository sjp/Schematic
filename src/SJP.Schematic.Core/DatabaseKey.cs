using System;
using System.Collections.Generic;
using System.ComponentModel;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// A database key constraint.
/// </summary>
/// <seealso cref="IDatabaseKey" />
public class DatabaseKey : IDatabaseKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseKey"/> class.
    /// </summary>
    /// <param name="name">A constraint name, if available.</param>
    /// <param name="keyType">The key constraint type.</param>
    /// <param name="columns">The columns covered by the key.</param>
    /// <param name="isEnabled">Whether the constraint is enabled.</param>
    /// <exception cref="ArgumentNullException"><paramref name="columns"/> is <c>null</c> or contains <c>null</c> values.</exception>
    /// <exception cref="ArgumentException"><paramref name="keyType"/> is an invalid enum value.</exception>
    public DatabaseKey(Option<Identifier> name, DatabaseKeyType keyType, IReadOnlyCollection<IDatabaseColumn> columns, bool isEnabled)
    {
        if (columns.NullOrEmpty() || columns.AnyNull())
            throw new ArgumentNullException(nameof(columns));
        if (!keyType.IsValid())
            throw new ArgumentException($"The { nameof(DatabaseKeyType) } provided must be a valid enum.", nameof(keyType));

        Name = name.Map(static n => Identifier.CreateQualifiedIdentifier(n.LocalName)); // strip to localname only
        KeyType = keyType;
        Columns = columns;
        IsEnabled = isEnabled;
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
    /// <value><c>true</c> if this object is enabled; otherwise, <c>false</c>.</value>
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