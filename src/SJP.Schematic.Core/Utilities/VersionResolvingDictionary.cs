using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Utilities
{
    public class VersionResolvingDictionary<T>
    {
        public VersionResolvingDictionary(IReadOnlyDictionary<Version, T> lookup)
        {
            if (lookup == null)
                throw new ArgumentNullException(nameof(lookup));
            if (lookup.Empty())
                throw new ArgumentException("At least one value must be present in the given lookup.", nameof(lookup));

            _lookup = lookup;
        }

        public T GetValue(Version version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            var versionKeys = _lookup.Keys.OrderBy(x => x).ToList();
            var firstVersion = versionKeys[0];
            if (version <= firstVersion)
                return _lookup[firstVersion];

            // we want to find the version that's *at least* the version
            // but we want to use the highest version possible
            versionKeys.Reverse();

            var matchingVersion = versionKeys.Find(v => version >= v);
            return _lookup[matchingVersion];
        }

        private readonly IReadOnlyDictionary<Version, T> _lookup;
    }
}
