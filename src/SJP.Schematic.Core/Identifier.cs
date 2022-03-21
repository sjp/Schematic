using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// Describes an identifier which represents any object with a database. In particular it enables behaviour such a scoping an object name to a schema.
/// </summary>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public sealed class Identifier : IEquatable<Identifier>, IComparable<Identifier>
{
    /// <summary>
    /// Creates an identifier that only contains an object's local name.
    /// </summary>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="localName"/> is <c>null</c>, empty, or whitespace.</exception>
    public Identifier(string localName)
    {
        if (localName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(localName));

        LocalName = localName;
    }

    /// <summary>
    /// Creates an identifier that contains an object's local name qualified by a schema.
    /// </summary>
    /// <param name="schema">The name of a schema.</param>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="schema"/> or <paramref name="localName"/> is <c>null</c>, empty, or whitespace.</exception>
    public Identifier(string schema, string localName)
    {
        if (schema.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(schema));
        if (localName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(localName));

        Schema = schema;
        LocalName = localName;
    }

    /// <summary>
    /// Creates an identifier that contains an object's local name qualified by a schema and database.
    /// </summary>
    /// <param name="database">The name of a database.</param>
    /// <param name="schema">The name of a schema.</param>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> or <paramref name="schema"/> or <paramref name="localName"/> is <c>null</c>, empty, or whitespace.</exception>
    public Identifier(string database, string schema, string localName)
    {
        if (database.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(database));
        if (schema.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(schema));
        if (localName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(localName));

        Database = database;
        Schema = schema;
        LocalName = localName;
    }

    /// <summary>
    /// Creates an identifier that contains an object's local name qualified by a schema, database and server.
    /// </summary>
    /// <param name="server">The name of a server.</param>
    /// <param name="database">The name of a database.</param>
    /// <param name="schema">The name of a schema.</param>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="server"/> or <paramref name="database"/> or <paramref name="schema"/> or <paramref name="localName"/> is <c>null</c>, empty, or whitespace.</exception>
    public Identifier(string server, string database, string schema, string localName)
    {
        if (server.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(server));
        if (database.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(database));
        if (schema.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(schema));
        if (localName.IsNullOrWhiteSpace())
            throw new ArgumentNullException(nameof(localName));

        Server = server;
        Database = database;
        Schema = schema;
        LocalName = localName;
    }

    /// <summary>
    /// Creates an identifier that creates the most qualified name given its input.
    /// </summary>
    /// <param name="server">The name of a server.</param>
    /// <param name="database">The name of a database.</param>
    /// <param name="schema">The name of a schema.</param>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException">Thrown when a parent component name is specified, but not one of its children.</exception>
    /// <remarks>This enables easy creation of identifiers when only a subset may be known in advance. For example, if only a schema and local name exists, the server and database name can be omitted (by providing <c>null</c>) arguments.</remarks>
    public static Identifier CreateQualifiedIdentifier(string? server, string? database, string? schema, string? localName)
    {
        var serverPresent = !server.IsNullOrWhiteSpace();
        var databasePresent = !database.IsNullOrWhiteSpace();
        var schemaPresent = !schema.IsNullOrWhiteSpace();
        var localNamePresent = !localName.IsNullOrWhiteSpace();

        var identifierKey = GetHashCode(server, database, schema, localName);
        if (Cache.TryGetValue(identifierKey, out var cachedReference) && cachedReference.TryGetTarget(out var cachedIdentifier))
            return cachedIdentifier;

        Identifier? result = null;
        if (serverPresent && databasePresent && schemaPresent && localNamePresent)
            result = new Identifier(server!, database!, schema!, localName!);
        else if (serverPresent)
            throw new ArgumentNullException(nameof(server), "A server name was provided, but other components are missing.");

        if (result == null)
        {
            if (databasePresent && schemaPresent && localNamePresent)
                result = new Identifier(database!, schema!, localName!);
            else if (databasePresent)
                throw new ArgumentNullException(nameof(database), "A database name was provided, but other components are missing.");
        }

        if (result == null)
        {
            if (schemaPresent && localNamePresent)
                result = new Identifier(schema!, localName!);
            else if (schemaPresent)
                throw new ArgumentNullException(nameof(schema), "A schema name was provided, but other components are missing.");
        }

        if (!localNamePresent)
            throw new ArgumentNullException(nameof(localName), "At least one component of an identifier must be provided.");
        if (result == null)
            result = new Identifier(localName!);

        if (Cache.TryGetValue(identifierKey, out var reference) && !reference.TryGetTarget(out _))
        {
            reference.SetTarget(result);
        }
        else
        {
            WeakReference<Identifier> addFactory(int _) => new(result!);
            WeakReference<Identifier> updateFactory(int _, WeakReference<Identifier> oldValue)
            {
                oldValue.SetTarget(result!);
                return oldValue;
            }
            Cache.AddOrUpdate(identifierKey, addFactory, updateFactory);
        }

        return result;
    }

    /// <summary>
    /// Creates an identifier that creates the most qualified name given its input.
    /// </summary>
    /// <param name="database">The name of a database.</param>
    /// <param name="schema">The name of a schema.</param>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException">Thrown when a parent component name is specified, but not one of its children.</exception>
    /// <remarks>This enables easy creation of identifiers when only a subset may be known in advance. For example, if only a schema and local name exists, the server and database name can be omitted (by providing <c>null</c>) arguments.</remarks>
    public static Identifier CreateQualifiedIdentifier(string? database, string? schema, string? localName) => CreateQualifiedIdentifier(null, database, schema, localName);

    /// <summary>
    /// Creates an identifier that creates the most qualified name given its input.
    /// </summary>
    /// <param name="schema">The name of a schema.</param>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException">Thrown when a parent component name is specified, but not one of its children.</exception>
    /// <remarks>This enables easy creation of identifiers when only a subset may be known in advance. For example, if only a schema and local name exists, the server and database name can be omitted (by providing <c>null</c>) arguments.</remarks>
    public static Identifier CreateQualifiedIdentifier(string? schema, string? localName) => CreateQualifiedIdentifier(null, null, schema, localName);

    /// <summary>
    /// Creates an identifier that creates the most qualified name given its input.
    /// </summary>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="localName"/> is <c>null</c>.</exception>
    public static Identifier CreateQualifiedIdentifier(string? localName) => CreateQualifiedIdentifier(null, null, null, localName);

    /// <summary>
    /// A convenience operator that creates an <see cref="Identifier"/> from a string.
    /// </summary>
    /// <param name="localName">An object name.</param>
    public static implicit operator Identifier(string localName)
    {
        var identifierKey = GetHashCode(null, null, null, localName);

        if (!Cache.TryGetValue(identifierKey, out var reference))
            reference = new WeakReference<Identifier>(new Identifier(localName));

        if (!reference.TryGetTarget(out var identifier))
        {
            identifier = new Identifier(localName);
            reference.SetTarget(identifier);
        }

        return identifier;
    }

    /// <summary>
    /// A server name.
    /// </summary>
    public string? Server { get; }

    /// <summary>
    /// A database name.
    /// </summary>
    public string? Database { get; }

    /// <summary>
    /// A schema name.
    /// </summary>
    public string? Schema { get; }

    /// <summary>
    /// An object name.
    /// </summary>
    public string LocalName { get; }

    /// <summary>
    /// Provides a string representation of the <see cref="Identifier"/>. Not intended to be used directly.
    /// </summary>
    /// <returns>A string representation of the <see cref="Identifier"/>.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString()
    {
        // not intended to be used for anything except debugging
        return DebuggerDisplay;
    }

    /// <summary>
    /// Checks whether two identifiers are equal using the default (ordinal) identifier comparer.
    /// </summary>
    /// <param name="a">A database identifier.</param>
    /// <param name="b">Another database identifier.</param>
    /// <returns><c>true</c> if all components of an identifier are equal; otherwise <c>false</c>.</returns>
    public static bool operator ==(Identifier? a, Identifier? b)
    {
        if (a is null && b is null)
            return true;
        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    /// <summary>
    /// Checks whether two identifiers are not equal using the default (ordinal) identifier comparer.
    /// </summary>
    /// <param name="a">A database identifier.</param>
    /// <param name="b">Another database identifier.</param>
    /// <returns><c>false</c> if all components of an identifier are equal; otherwise <c>true</c>.</returns>
    public static bool operator !=(Identifier? a, Identifier? b)
    {
        if (a is null && b is null)
            return false;
        if (a is null || b is null)
            return true;

        return !a.Equals(b);
    }

    /// <summary>
    /// Compares two specified <see cref="Identifier"/> objects and returns true when the first operand follows the second when sorted.
    /// </summary>
    /// <param name="a">A database identifier.</param>
    /// <param name="b">Another database identifier.</param>
    /// <returns><c>true</c> if <paramref name="a"/> follows <paramref name="b"/> when both are sorted; otherwise <c>false</c>.</returns>
    public static bool operator >(Identifier a, Identifier b) => IdentifierComparer.Ordinal.Compare(a, b) > 0;

    /// <summary>
    /// Compares two specified <see cref="Identifier"/> objects and returns true when the first operand precedes the second when sorted.
    /// </summary>
    /// <param name="a">A database identifier.</param>
    /// <param name="b">Another database identifier.</param>
    /// <returns><c>true</c> if <paramref name="a"/> precedes <paramref name="b"/> when both are sorted; otherwise <c>false</c>.</returns>
    public static bool operator <(Identifier a, Identifier b) => IdentifierComparer.Ordinal.Compare(a, b) < 0;

    /// <summary>
    /// Compares two specified <see cref="Identifier"/> objects and returns true when the first operand follows or is equal to the second when sorted.
    /// </summary>
    /// <param name="a">A database identifier.</param>
    /// <param name="b">Another database identifier.</param>
    /// <returns><c>true</c> if <paramref name="a"/> follows or is equal to <paramref name="b"/> when both are sorted; otherwise <c>false</c>.</returns>
    public static bool operator >=(Identifier a, Identifier b) => IdentifierComparer.Ordinal.Compare(a, b) >= 0;

    /// <summary>
    /// Compares two specified <see cref="Identifier"/> objects and returns true when the first operand precedes or is equal to the second when sorted.
    /// </summary>
    /// <param name="a">A database identifier.</param>
    /// <param name="b">Another database identifier.</param>
    /// <returns><c>true</c> if <paramref name="a"/> precedes or is equal to <paramref name="b"/> when both are sorted; otherwise <c>false</c>.</returns>
    public static bool operator <=(Identifier a, Identifier b) => IdentifierComparer.Ordinal.Compare(a, b) <= 0;

    /// <summary>
    /// Determines whether the specified identifier is equal to the current identifier using the default (ordinal) identifier comparer.
    /// </summary>
    /// <param name="other">The identifier to compare with the current identifier.</param>
    /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
    public bool Equals(Identifier? other) => IdentifierComparer.Ordinal.Equals(this, other);

    /// <summary>
    /// Determines whether the specified object is equal to the current identifier using the default (ordinal) identifier comparer.
    /// </summary>
    /// <param name="obj">The object to compare with the current identifier.</param>
    /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (obj is not Identifier other)
            return false;

        return Equals(other);
    }

    /// <summary>
    /// A hash code for the current identifier using the default (ordinal) identifier comparer.
    /// </summary>
    /// <returns>A hash code.</returns>
    public override int GetHashCode() => IdentifierComparer.Ordinal.GetHashCode(this);

    /// <summary>
    /// Compares this instance with a specified <see cref="Identifier"/> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="Identifier"/>.
    /// </summary>
    /// <param name="other">An identifier to compare with the current identifier.</param>
    /// <returns>A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="other"/> parameter.</returns>
    public int CompareTo(Identifier? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (other == null)
            return 1;

        return IdentifierComparer.Ordinal.Compare(this, other);
    }

    private string DebuggerDisplay
    {
        get
        {
            var builder = StringBuilderCache.Acquire();

            // always used except for LocalName, if available
            const string separator = ", ";

            if (!Server.IsNullOrWhiteSpace())
                builder.Append("Server = ").Append(Server).Append(separator);
            if (!Database.IsNullOrWhiteSpace())
                builder.Append("Database = ").Append(Database).Append(separator);
            if (!Schema.IsNullOrWhiteSpace())
                builder.Append("Schema = ").Append(Schema).Append(separator);
            if (!LocalName.IsNullOrWhiteSpace())
                builder.Append("LocalName = ").Append(LocalName);

            return builder.GetStringAndRelease();
        }
    }

    private static int GetHashCode(string? server, string? database, string? schema, string? localName)
    {
        return HashCode.Combine(
            server != null ? Comparer.GetHashCode(server) : 0,
            database != null ? Comparer.GetHashCode(database) : 0,
            schema != null ? Comparer.GetHashCode(schema) : 0,
            localName != null ? Comparer.GetHashCode(localName) : 0
        );
    }

    private static readonly ConcurrentDictionary<int, WeakReference<Identifier>> Cache = new();
    private static readonly StringComparer Comparer = StringComparer.Ordinal;
}