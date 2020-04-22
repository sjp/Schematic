using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace SJP.Schematic.Tests.Utilities
{
    public sealed class LineEndingInvariantStringComparer : IEqualityComparer<string>
    {
        private static readonly Regex _lineEndingRegex = new Regex("\r\n|\n\r|\n|\r");
        private const string Crlf = "\r\n";

        private readonly StringComparer _comparer;

        public LineEndingInvariantStringComparer()
            : this(StringComparison.Ordinal)
        {
        }

        public LineEndingInvariantStringComparer(StringComparison comparison)
        {
            _comparer = StringComparer.FromComparison(comparison);
        }

        public static LineEndingInvariantStringComparer CurrentCulture { get; } = new LineEndingInvariantStringComparer(StringComparison.CurrentCulture);

        public static LineEndingInvariantStringComparer CurrentCultureIgnoreCase { get; } = new LineEndingInvariantStringComparer(StringComparison.CurrentCultureIgnoreCase);

        public static LineEndingInvariantStringComparer Ordinal { get; } = new LineEndingInvariantStringComparer(StringComparison.Ordinal);

        public static LineEndingInvariantStringComparer OrdinalIgnoreCase { get; } = new LineEndingInvariantStringComparer(StringComparison.OrdinalIgnoreCase);

        public static LineEndingInvariantStringComparer InvariantCulture { get; } = new LineEndingInvariantStringComparer(StringComparison.InvariantCulture);

        public static LineEndingInvariantStringComparer InvariantCultureIgnoreCase { get; } = new LineEndingInvariantStringComparer(StringComparison.InvariantCultureIgnoreCase);

        public bool Equals([AllowNull] string x, [AllowNull] string y)
        {
            if (x != null)
                x = NormalizeNewlines(x);
            if (y != null)
                y = NormalizeNewlines(y);

            return _comparer.Equals(x, y);
        }

        public int GetHashCode([DisallowNull] string obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return _comparer.GetHashCode(NormalizeNewlines(obj));
        }

        private static string NormalizeNewlines(string input) => _lineEndingRegex.Replace(input, Crlf);
    }
}
