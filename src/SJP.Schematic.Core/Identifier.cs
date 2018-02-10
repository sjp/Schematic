using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Describes an identifier which represents any object with a database. In particular it enables behaviour such a scoping an object name to a schema.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
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
        /// <remarks>This enables easy creation of identifiers when only a subset may be known in advance. For example, if only a schema and local name exists, the server and database name can be ommitted (by providing <c>null</c>) arguments.</remarks>
        public static Identifier CreateQualifiedIdentifier(string server, string database, string schema, string localName)
        {
            var serverPresent = !server.IsNullOrWhiteSpace();
            var databasePresent = !database.IsNullOrWhiteSpace();
            var schemaPresent = !schema.IsNullOrWhiteSpace();
            var localNamePresent = !localName.IsNullOrWhiteSpace();

            if (serverPresent && databasePresent && schemaPresent && localNamePresent)
                return new Identifier(server, database, schema, localName);
            else if (serverPresent)
                throw new ArgumentNullException("A server name was provided, but other components are missing.");

            if (databasePresent && schemaPresent && localNamePresent)
                return new Identifier(database, schema, localName);
            else if (databasePresent)
                throw new ArgumentNullException("A database name was provided, but other components are missing.");

            if (schemaPresent && localNamePresent)
                return new Identifier(schema, localName);
            else if (schemaPresent)
                throw new ArgumentNullException("A schema name was provided, but other components are missing.");

            if (!localNamePresent)
                throw new ArgumentNullException("At least one component of an identifier must be provided.");

            return new Identifier(localName);
        }

        /// <summary>
        /// Creates an identifier that creates the most qualified name given its input.
        /// </summary>
        /// <param name="database">The name of a database.</param>
        /// <param name="schema">The name of a schema.</param>
        /// <param name="localName">An object name.</param>
        /// <exception cref="ArgumentNullException">Thrown when a parent component name is specified, but not one of its children.</exception>
        /// <remarks>This enables easy creation of identifiers when only a subset may be known in advance. For example, if only a schema and local name exists, the server and database name can be ommitted (by providing <c>null</c>) arguments.</remarks>
        public static Identifier CreateQualifiedIdentifier(string database, string schema, string localName) => CreateQualifiedIdentifier(null, database, schema, localName);

        /// <summary>
        /// Creates an identifier that creates the most qualified name given its input.
        /// </summary>
        /// <param name="schema">The name of a schema.</param>
        /// <param name="localName">An object name.</param>
        /// <exception cref="ArgumentNullException">Thrown when a parent component name is specified, but not one of its children.</exception>
        /// <remarks>This enables easy creation of identifiers when only a subset may be known in advance. For example, if only a schema and local name exists, the server and database name can be ommitted (by providing <c>null</c>) arguments.</remarks>
        public static Identifier CreateQualifiedIdentifier(string schema, string localName) => CreateQualifiedIdentifier(null, null, schema, localName);

        /// <summary>
        /// Creates an identifier that creates the most qualified name given its input.
        /// </summary>
        /// <param name="localName">An object name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="localName"/> is <c>null</c>.</exception>
        public static Identifier CreateQualifiedIdentifier(string localName) => CreateQualifiedIdentifier(null, null, null, localName);

        /// <summary>
        /// A convenience operator that creates an <see cref="Identifier"/> from a string.
        /// </summary>
        /// <param name="localName">An object name.</param>
        public static implicit operator Identifier(string localName) => new Identifier(localName);

        /// <summary>
        /// A server name.
        /// </summary>
        public string Server { get; }

        /// <summary>
        /// A database name.
        /// </summary>
        public string Database { get; }

        /// <summary>
        /// A schema name.
        /// </summary>
        public string Schema { get; }

        /// <summary>
        /// An object name.
        /// </summary>
        public string LocalName { get; }

        /// <summary>
        /// Provides a string representation of the <see cref="Identifier"/>. Not intended to be used directly.
        /// </summary>
        /// <returns>A string representation of the <see cref="Identifier"/>.</returns>
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
        public static bool operator ==(Identifier a, Identifier b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (ReferenceEquals(a, null) ^ ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        /// <summary>
        /// Checks whether two identifiers are not equal using the default (ordinal) identifier comparer.
        /// </summary>
        /// <param name="a">A database identifier.</param>
        /// <param name="b">Another database identifier.</param>
        /// <returns><c>false</c> if all components of an identifier are equal; otherwise <c>true</c>.</returns>
        public static bool operator !=(Identifier a, Identifier b)
        {
            if (ReferenceEquals(a, b))
                return false;
            if (ReferenceEquals(a, null) ^ ReferenceEquals(b, null))
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
        /// Determines whether the specified identifier is equal to the current identifier using the default (ordinal) identifier comparer.
        /// </summary>
        /// <param name="other">The identifier to compare with the current identifier.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(Identifier other) => IdentifierComparer.Ordinal.Equals(this, other);

        /// <summary>
        /// Determines whether the specified object is equal to the current identifier using the default (ordinal) identifier comparer.
        /// </summary>
        /// <param name="obj">The object to compare with the current identifier.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as Identifier;
            if (other == null)
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
        public int CompareTo(Identifier other)
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
                var pieces = new List<string>();

                if (!Server.IsNullOrWhiteSpace())
                    pieces.Add($"Server = { Server }");
                if (!Database.IsNullOrWhiteSpace())
                    pieces.Add($"Database = { Database }");
                if (!Schema.IsNullOrWhiteSpace())
                    pieces.Add($"Schema = { Schema }");
                if (!LocalName.IsNullOrWhiteSpace())
                    pieces.Add($"LocalName = { LocalName }");

                return pieces.Join(", ");
            }
        }
    }
}
