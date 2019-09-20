using System;
using System.ComponentModel;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Dot
{
    internal sealed class DotIdentifier : IEquatable<DotIdentifier>
    {
        public DotIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            _identifier = "\"" + identifier.Replace("\"", "\\\"") + "\"";
            _hash = _identifier.GetHashCode();
        }

        public override string ToString() => _identifier;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => _hash;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as DotIdentifier);
        }

        public bool Equals(DotIdentifier other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return _identifier == other.ToString();
        }

        private readonly string _identifier;
        private readonly int _hash;
    }
}
