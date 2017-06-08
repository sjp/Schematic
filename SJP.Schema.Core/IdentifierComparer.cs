using System;
using System.Collections.Generic;

namespace SJP.Schema.Core
{

    //
    //
    //
    //
    //
    //
    //
    /*
     *
     *
     IDEA FOR QUOTING:
        If a dialect provider can have a static method for quoting, then pass that in to a method or derive from Name
        i.e.

    ToString()
    {
        DialectProvider.QuotingMethod("a", "b", "c")
    }


    that way we can get a generic instance always and a specific instance always too.
    Just means we need to derive from Name. Not bad...
 *
     *
     */

    public class IdentifierComparer : IEqualityComparer<Identifier>, IComparer<Identifier>
    {
        public IdentifierComparer(StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (!Enum.IsDefined(_strComparisonType, comparison))
                throw new InvalidOperationException($"The { nameof(StringComparison) } value is not defined.");

            _comparer = GetStringComparer(comparison);
        }

        public IdentifierComparer(StringComparer comparer) // can't use IComparer or IEqualityComparer because we need both
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        public bool Equals(Identifier x, Identifier y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (ReferenceEquals(x, null) ^ ReferenceEquals(y, null))
                return false;

            // both must be not null at this point
            return _comparer.Equals(x.Server, y.Server)
                && _comparer.Equals(x.Database, y.Database)
                && _comparer.Equals(x.Schema, y.Schema)
                && _comparer.Equals(x.LocalName, y.LocalName);
        }

        public int GetHashCode(Identifier obj)
        {
            if (obj == null)
                return 0;

            unchecked
            {
                var hash = 17;
                hash = (hash * 23) + (obj.Server != null ? _comparer.GetHashCode(obj.Server) : 0);
                hash = (hash * 23) + (obj.Database != null ? _comparer.GetHashCode(obj.Database) : 0);
                hash = (hash * 23) + (obj.Schema != null ? _comparer.GetHashCode(obj.Schema) : 0);
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

            var result = _comparer.Compare(x.Server, y.Server);
            if (result != 0)
                return result;

            result = _comparer.Compare(x.Database, y.Database);
            if (result != 0)
                return result;

            result = _comparer.Compare(x.Schema, y.Schema);
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

        private readonly StringComparer _comparer;

        private static readonly Type _strComparisonType = typeof(StringComparison);
    }
}
