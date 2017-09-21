using System;
using System.Collections.Generic;
using EnumsNET;

namespace SJP.Schematic.Core
{
    public class IdentifierComparer : IEqualityComparer<Identifier>, IComparer<Identifier>
    {
        public IdentifierComparer(StringComparison comparison = StringComparison.OrdinalIgnoreCase, string defaultServer = null, string defaultDatabase = null, string defaultSchema = null)
        {
            if (!comparison.IsValid())
                throw new ArgumentException($"The { nameof(StringComparison) } provided must be a valid enum.", nameof(comparison));

            _comparer = GetStringComparer(comparison);
            _defaultServer = defaultServer.IsNullOrWhiteSpace() ? null : defaultServer;
            _defaultDatabase = defaultDatabase.IsNullOrWhiteSpace() ? null : defaultDatabase;
            _defaultSchema = defaultSchema.IsNullOrWhiteSpace() ? null : defaultSchema;
        }

        public IdentifierComparer(StringComparer comparer, string defaultServer = null, string defaultDatabase = null, string defaultSchema = null) // can't use IComparer or IEqualityComparer because we need both
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _defaultServer = defaultServer.IsNullOrWhiteSpace() ? null : defaultServer;
            _defaultDatabase = defaultDatabase.IsNullOrWhiteSpace() ? null : defaultDatabase;
            _defaultSchema = defaultSchema.IsNullOrWhiteSpace() ? null : defaultSchema;
        }

        public bool Equals(Identifier x, Identifier y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) ^ ReferenceEquals(y, null))
                return false;

            // both must be not null at this point
            return _comparer.Equals(x.Server ?? _defaultServer, y.Server ?? _defaultServer)
                && _comparer.Equals(x.Database ?? _defaultDatabase, y.Database ?? _defaultDatabase)
                && _comparer.Equals(x.Schema ?? _defaultSchema, y.Schema ?? _defaultSchema)
                && _comparer.Equals(x.LocalName, y.LocalName);
        }

        public int GetHashCode(Identifier obj)
        {
            if (obj == null)
                return 0;

            unchecked
            {
                var hash = 17;
                hash = (hash * 23) + (obj.Server != null ? _comparer.GetHashCode(obj.Server) : _defaultServer != null ? _comparer.GetHashCode(_defaultServer) : 0);
                hash = (hash * 23) + (obj.Database != null ? _comparer.GetHashCode(obj.Database) : _defaultDatabase != null ? _comparer.GetHashCode(_defaultDatabase) : 0);
                hash = (hash * 23) + (obj.Schema != null ? _comparer.GetHashCode(obj.Schema) : _defaultSchema != null ? _comparer.GetHashCode(_defaultSchema) : 0);
                hash = (hash * 23) + (obj.LocalName != null ? _comparer.GetHashCode(obj.LocalName) : 0);
                return hash;
            }
        }

        public int Compare(Identifier x, Identifier y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            var result = _comparer.Compare(x.Server ?? _defaultServer, y.Server ?? _defaultServer);
            if (result != 0)
                return result;

            result = _comparer.Compare(x.Database ?? _defaultDatabase, y.Database ?? _defaultDatabase);
            if (result != 0)
                return result;

            result = _comparer.Compare(x.Schema ?? _defaultSchema, y.Schema ?? _defaultSchema);
            if (result != 0)
                return result;

            return _comparer.Compare(x.LocalName, y.LocalName);
        }

        public static IdentifierComparer CurrentCulture { get; } = new IdentifierComparer(StringComparer.CurrentCulture);

        public static IdentifierComparer CurrentCultureIgnoreCase { get; } = new IdentifierComparer(StringComparer.CurrentCultureIgnoreCase);

        public static IdentifierComparer Ordinal { get; } = new IdentifierComparer(StringComparer.Ordinal);

        public static IdentifierComparer OrdinalIgnoreCase { get; } = new IdentifierComparer(StringComparer.OrdinalIgnoreCase);

        private static StringComparer GetStringComparer(StringComparison comparison)
        {
            switch (comparison)
            {
                case StringComparison.CurrentCulture:
                    return StringComparer.CurrentCulture;
                case StringComparison.CurrentCultureIgnoreCase:
                    return StringComparer.CurrentCultureIgnoreCase;
                case StringComparison.Ordinal:
                    return StringComparer.Ordinal;
                case StringComparison.OrdinalIgnoreCase:
                    return StringComparer.OrdinalIgnoreCase;
                default:
                    throw new ArgumentOutOfRangeException(nameof(comparison));
            }
        }

        private readonly string _defaultSchema;
        private readonly string _defaultDatabase;
        private readonly string _defaultServer;
        private readonly StringComparer _comparer;
    }
}
