using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SJP.Schematic.Core
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Identifier : IEquatable<Identifier>, IComparable<Identifier>
    {
        public Identifier(string localName)
        {
            if (localName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(localName));

            _localName = localName;
        }

        public Identifier(string schema, string localName)
        {
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));
            if (localName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(localName));

            _schemaName = schema;
            _localName = localName;
        }

        public Identifier(string database, string schema, string localName)
        {
            if (database.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(database));
            if (schema.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schema));
            if (localName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(localName));

            _databaseName = database;
            _schemaName = schema;
            _localName = localName;
        }

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

            _serverName = server;
            _databaseName = database;
            _schemaName = schema;
            _localName = localName;
        }

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

        public static Identifier CreateQualifiedIdentifier(string database, string schema, string localName) => CreateQualifiedIdentifier(null, database, schema, localName);

        public static Identifier CreateQualifiedIdentifier(string schema, string localName) => CreateQualifiedIdentifier(null, null, schema, localName);

        public static Identifier CreateQualifiedIdentifier(string localName) => CreateQualifiedIdentifier(null, null, null, localName);

        // needed for inheritance only
        protected Identifier()
        {
        }

        public static implicit operator Identifier(string localName) => new Identifier(localName);

        public string Server => _serverName;

        public string Database => _databaseName;

        public string Schema => _schemaName;

        public string LocalName => _localName;

        public override string ToString()
        {
            // not intended to be used for anything except debugging
            return DebuggerDisplay;
        }

        public static bool operator ==(Identifier a, Identifier b)
        {
            if (ReferenceEquals(a, b))
                return true;
            if (ReferenceEquals(a, null) ^ ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(Identifier a, Identifier b)
        {
            if (ReferenceEquals(a, b))
                return false;
            if (ReferenceEquals(a, null) ^ ReferenceEquals(b, null))
                return true;

            return !a.Equals(b);
        }

        public static bool operator >(Identifier a, Identifier b) => IdentifierComparer.Ordinal.Compare(a, b) > 0;

        public static bool operator <(Identifier a, Identifier b) => IdentifierComparer.Ordinal.Compare(a, b) < 0;

        public bool Equals(Identifier other) => IdentifierComparer.Ordinal.Equals(this, other);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var other = obj as Identifier;
            if (other == null)
                return false;

            return Equals(other);
        }

        public override int GetHashCode() => IdentifierComparer.Ordinal.GetHashCode(this);

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

        protected string _serverName;
        protected string _databaseName;
        protected string _schemaName;
        protected string _localName;
    }

    public class ServerIdentifier : Identifier
    {
        public ServerIdentifier(string serverName)
        {
            if (serverName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(serverName));

            _serverName = serverName;
        }
    }

    public class DatabaseIdentifier : Identifier
    {
        public DatabaseIdentifier(string databaseName)
        {
            if (databaseName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(databaseName));

            _databaseName = databaseName;
        }
    }

    public class SchemaIdentifier : Identifier
    {
        public SchemaIdentifier(string schemaName)
        {
            if (schemaName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(schemaName));

            _schemaName = schemaName;
        }
    }

    public class LocalIdentifier : Identifier
    {
        public LocalIdentifier(string localName)
        {
            if (localName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(localName));

            _localName = localName;
        }
    }
}
