using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core
{
    /// <summary>
    /// Represents an <see cref="Identifier"/> comparison operation that uses specific case and culture-based or ordinal comparison rules.
    /// </summary>
    public sealed class IdentifierComparer : IEqualityComparer<Identifier>, IComparer<Identifier>
    {
        public IdentifierComparer(StringComparison comparison = StringComparison.OrdinalIgnoreCase, string defaultServer = null, string defaultDatabase = null, string defaultSchema = null)
        {
            if (!comparison.IsValid())
                throw new ArgumentException($"The { nameof(StringComparison) } provided must be a valid enum.", nameof(comparison));

            _comparer = _stringComparerLookup[comparison];
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
            if (x is null ^ y is null)
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
                return (hash * 23) + (obj.LocalName != null ? _comparer.GetHashCode(obj.LocalName) : 0);
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

        public static IdentifierComparer InvariantCulture { get; } = new IdentifierComparer(StringComparer.InvariantCulture);

        public static IdentifierComparer InvariantCultureIgnoreCase { get; } = new IdentifierComparer(StringComparer.InvariantCultureIgnoreCase);

        private readonly string _defaultSchema;
        private readonly string _defaultDatabase;
        private readonly string _defaultServer;
        private readonly StringComparer _comparer;

        private readonly static IReadOnlyDictionary<StringComparison, StringComparer> _stringComparerLookup = new Dictionary<StringComparison, StringComparer>
        {
            [StringComparison.CurrentCulture] = StringComparer.CurrentCulture,
            [StringComparison.CurrentCultureIgnoreCase] = StringComparer.CurrentCultureIgnoreCase,
            [StringComparison.Ordinal] = StringComparer.Ordinal,
            [StringComparison.OrdinalIgnoreCase] = StringComparer.OrdinalIgnoreCase,
            [StringComparison.InvariantCulture] = StringComparer.InvariantCulture,
            [StringComparison.InvariantCultureIgnoreCase] = StringComparer.InvariantCultureIgnoreCase
        };
    }
}
