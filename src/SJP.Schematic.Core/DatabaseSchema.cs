using System;
using System.ComponentModel;
using System.Diagnostics;
using LanguageExt;

namespace SJP.Schematic.Core;

/// <summary>
/// Implements a database schema, i.e. the namespace that database objects are declared within.
/// </summary>
/// <seealso cref="IDatabaseSchema" />
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public class DatabaseSchema : IDatabaseSchema
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSchema"/> class.
    /// </summary>
    /// <param name="schemaName">The name of the schema.</param>
    /// <param name="owner">The principal that owns the schema, if known.</param>
    /// <param name="isDefault">Whether this is the schema that unqualified names resolve to.</param>
    /// <param name="isSystem">Whether the schema is declared by the database rather than by a user.</param>
    /// <exception cref="ArgumentNullException"><paramref name="schemaName"/> is <see langword="null" />.</exception>
    public DatabaseSchema(Identifier schemaName, Option<string> owner, bool isDefault, bool isSystem)
    {
        Name = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
        Owner = owner;
        IsDefault = isDefault;
        IsSystem = isSystem;
    }

    /// <summary>
    /// The name of the schema.
    /// </summary>
    /// <value>The schema name.</value>
    public Identifier Name { get; }

    /// <summary>
    /// The name of the principal that owns the schema, when the database records one.
    /// </summary>
    /// <value>The owner of the schema, if available.</value>
    public Option<string> Owner { get; }

    /// <summary>
    /// Whether this is the schema that unqualified object names resolve to.
    /// </summary>
    /// <value><see langword="true" /> if this is the default schema; otherwise <see langword="false" />.</value>
    public bool IsDefault { get; }

    /// <summary>
    /// Whether the schema is one that the database itself declares rather than one declared by a user.
    /// </summary>
    /// <value><see langword="true" /> if this is a system schema; otherwise <see langword="false" />.</value>
    public bool IsSystem { get; }

    /// <summary>
    /// Returns a string that provides a basic string representation of this object.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents this instance.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => DebuggerDisplay;

    private string DebuggerDisplay => "Schema: " + Name.LocalName;
}
