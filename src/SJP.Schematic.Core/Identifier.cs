using System;
using System.ComponentModel;
using System.Diagnostics;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core;

/// <summary>
/// Describes an identifier which represents any object within a database. In particular it enables behaviour such as scoping an object name to a schema.
/// </summary>
[DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
public sealed record Identifier : IComparable<Identifier>, IComparable
{
    /// <summary>
    /// Creates an identifier that only contains an object's local name.
    /// </summary>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="localName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="localName"/> is empty or whitespace.</exception>
    public Identifier(string localName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localName);

        LocalName = localName;
    }

    /// <summary>
    /// Creates an identifier that contains an object's local name qualified by a schema.
    /// </summary>
    /// <param name="schema">The name of a schema.</param>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="schema"/> or <paramref name="localName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="schema"/> or <paramref name="localName"/> is empty or whitespace.</exception>
    public Identifier(string schema, string localName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);
        ArgumentException.ThrowIfNullOrWhiteSpace(localName);

        Schema = schema;
        LocalName = localName;
    }

    /// <summary>
    /// Creates an identifier that contains an object's local name qualified by a schema and database.
    /// </summary>
    /// <param name="database">The name of a database.</param>
    /// <param name="schema">The name of a schema.</param>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="database"/> or <paramref name="schema"/> or <paramref name="localName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="database"/> or <paramref name="schema"/> or <paramref name="localName"/> is empty or whitespace.</exception>
    public Identifier(string database, string schema, string localName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);
        ArgumentException.ThrowIfNullOrWhiteSpace(localName);

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
    /// <exception cref="ArgumentNullException"><paramref name="server"/> or <paramref name="database"/> or <paramref name="schema"/> or <paramref name="localName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="server"/> or <paramref name="database"/> or <paramref name="schema"/> or <paramref name="localName"/> is empty or whitespace.</exception>
    public Identifier(string server, string database, string schema, string localName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(server);
        ArgumentException.ThrowIfNullOrWhiteSpace(database);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);
        ArgumentException.ThrowIfNullOrWhiteSpace(localName);

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
    /// <exception cref="ArgumentNullException">A parent component name is specified, but one of its children is <see langword="null" />. Also thrown when <paramref name="localName"/> is <see langword="null" /> and no other component is present.</exception>
    /// <exception cref="ArgumentException">A parent component name is specified, but one of its children is empty or whitespace. Also thrown when <paramref name="localName"/> is empty or whitespace and no other component is present.</exception>
    /// <remarks>This enables easy creation of identifiers when only a subset may be known in advance. For example, if only a schema and local name exists, the server and database name can be omitted (by providing <see langword="null" />) arguments. Empty and whitespace-only values for the optional leading components are treated as if the component was absent, but a missing <paramref name="localName"/> is always an error.</remarks>
    public static Identifier CreateQualifiedIdentifier(string? server, string? database, string? schema, string? localName)
    {
        var serverPresent = !server.IsNullOrWhiteSpace();
        var databasePresent = !database.IsNullOrWhiteSpace();
        var schemaPresent = !schema.IsNullOrWhiteSpace();
        var localNamePresent = !localName.IsNullOrWhiteSpace();

        if (serverPresent && !databasePresent)
            throw MissingComponent(database, nameof(database), "A server name was provided, but a database name is missing.");
        if ((serverPresent || databasePresent) && !schemaPresent)
            throw MissingComponent(schema, nameof(schema), "A database name was provided, but a schema name is missing.");
        if ((serverPresent || databasePresent || schemaPresent) && !localNamePresent)
            throw MissingComponent(localName, nameof(localName), "A schema name was provided, but a local name is missing.");
        if (!localNamePresent)
            throw MissingComponent(localName, nameof(localName), "At least one component of an identifier must be provided.");

        if (serverPresent)
            return new Identifier(server!, database!, schema!, localName!);
        if (databasePresent)
            return new Identifier(database!, schema!, localName!);
        if (schemaPresent)
            return new Identifier(schema!, localName!);

        return new Identifier(localName!);
    }

    /// <summary>
    /// Creates an identifier that creates the most qualified name given its input.
    /// </summary>
    /// <param name="database">The name of a database.</param>
    /// <param name="schema">The name of a schema.</param>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException">A parent component name is specified, but one of its children is <see langword="null" />. Also thrown when <paramref name="localName"/> is <see langword="null" /> and no other component is present.</exception>
    /// <exception cref="ArgumentException">A parent component name is specified, but one of its children is empty or whitespace. Also thrown when <paramref name="localName"/> is empty or whitespace and no other component is present.</exception>
    /// <remarks>This enables easy creation of identifiers when only a subset may be known in advance. For example, if only a schema and local name exists, the server and database name can be omitted (by providing <see langword="null" />) arguments.</remarks>
    public static Identifier CreateQualifiedIdentifier(string? database, string? schema, string? localName) => CreateQualifiedIdentifier(null, database, schema, localName);

    /// <summary>
    /// Creates an identifier that creates the most qualified name given its input.
    /// </summary>
    /// <param name="schema">The name of a schema.</param>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException">A parent component name is specified, but one of its children is <see langword="null" />. Also thrown when <paramref name="localName"/> is <see langword="null" /> and no other component is present.</exception>
    /// <exception cref="ArgumentException">A parent component name is specified, but one of its children is empty or whitespace. Also thrown when <paramref name="localName"/> is empty or whitespace and no other component is present.</exception>
    /// <remarks>This enables easy creation of identifiers when only a subset may be known in advance. For example, if only a schema and local name exists, the server and database name can be omitted (by providing <see langword="null" />) arguments.</remarks>
    public static Identifier CreateQualifiedIdentifier(string? schema, string? localName) => CreateQualifiedIdentifier(null, null, schema, localName);

    /// <summary>
    /// Creates an identifier that creates the most qualified name given its input.
    /// </summary>
    /// <param name="localName">An object name.</param>
    /// <exception cref="ArgumentNullException"><paramref name="localName"/> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException"><paramref name="localName"/> is empty or whitespace.</exception>
    public static Identifier CreateQualifiedIdentifier(string? localName) => CreateQualifiedIdentifier(null, null, null, localName);

    /// <summary>
    /// A convenience operator that creates an <see cref="Identifier"/> from a string.
    /// </summary>
    /// <param name="localName">An object name.</param>
    public static implicit operator Identifier(string localName)
        => new Identifier(localName);

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
    /// Compares two specified <see cref="Identifier"/> objects and returns true when the first operand follows the second when sorted.
    /// </summary>
    /// <param name="a">A database identifier.</param>
    /// <param name="b">Another database identifier.</param>
    /// <returns><see langword="true" /> if <paramref name="a"/> follows <paramref name="b"/> when both are sorted; otherwise <see langword="false" />.</returns>
    public static bool operator >(Identifier a, Identifier b) => IdentifierComparer.Ordinal.Compare(a, b) > 0;

    /// <summary>
    /// Compares two specified <see cref="Identifier"/> objects and returns true when the first operand precedes the second when sorted.
    /// </summary>
    /// <param name="a">A database identifier.</param>
    /// <param name="b">Another database identifier.</param>
    /// <returns><see langword="true" /> if <paramref name="a"/> precedes <paramref name="b"/> when both are sorted; otherwise <see langword="false" />.</returns>
    public static bool operator <(Identifier a, Identifier b) => IdentifierComparer.Ordinal.Compare(a, b) < 0;

    /// <summary>
    /// Compares two specified <see cref="Identifier"/> objects and returns true when the first operand follows or is equal to the second when sorted.
    /// </summary>
    /// <param name="a">A database identifier.</param>
    /// <param name="b">Another database identifier.</param>
    /// <returns><see langword="true" /> if <paramref name="a"/> follows or is equal to <paramref name="b"/> when both are sorted; otherwise <see langword="false" />.</returns>
    public static bool operator >=(Identifier a, Identifier b) => IdentifierComparer.Ordinal.Compare(a, b) >= 0;

    /// <summary>
    /// Compares two specified <see cref="Identifier"/> objects and returns true when the first operand precedes or is equal to the second when sorted.
    /// </summary>
    /// <param name="a">A database identifier.</param>
    /// <param name="b">Another database identifier.</param>
    /// <returns><see langword="true" /> if <paramref name="a"/> precedes or is equal to <paramref name="b"/> when both are sorted; otherwise <see langword="false" />.</returns>
    public static bool operator <=(Identifier a, Identifier b) => IdentifierComparer.Ordinal.Compare(a, b) <= 0;

    /// <summary>
    /// Compares this instance with a specified <see cref="Identifier"/> object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified <see cref="Identifier"/>.
    /// </summary>
    /// <param name="other">An identifier to compare with the current identifier.</param>
    /// <returns>A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="other"/> parameter.</returns>
    public int CompareTo(Identifier? other)
    {
        if (ReferenceEquals(this, other))
            return 0;

        if (other is null)
            return 1;

        return IdentifierComparer.Ordinal.Compare(this, other);
    }

    /// <summary>
    /// Compares this instance with a specified object and indicates whether this instance precedes, follows, or appears in the same position in the sort order as the specified object.
    /// </summary>
    /// <param name="obj">An object to compare with the current identifier.</param>
    /// <returns>A 32-bit signed integer that indicates whether this instance precedes, follows, or appears in the same position in the sort order as the <paramref name="obj"/> parameter.</returns>
    /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="Identifier"/>.</exception>
    public int CompareTo(object? obj)
    {
        if (obj is null)
            return 1;

        if (obj is not Identifier other)
            throw new ArgumentException($"The object to compare must be an {nameof(Identifier)}.", nameof(obj));

        return CompareTo(other);
    }

    // matches the exception types thrown by the constructors, i.e. null is distinguished from empty/whitespace
    private static ArgumentException MissingComponent(string? value, string paramName, string message)
    {
        return value is null
            ? new ArgumentNullException(paramName, message)
            : new ArgumentException(message, paramName);
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
}