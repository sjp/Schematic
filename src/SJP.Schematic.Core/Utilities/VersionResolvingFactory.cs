using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Utilities
{
    public class VersionResolvingFactory<T> : IVersionedLookup<T>
    {
        public VersionResolvingFactory(IReadOnlyDictionary<Version, Func<T>> lookup)
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
            {
                var resultFactory = _lookup[firstVersion];
                return resultFactory.Invoke();
            }

            // we want to find the version that's *at least* the version
            // but we want to use the highest version possible
            versionKeys.Reverse();

            var matchingVersion = versionKeys.Find(v => version >= v);
            var result = _lookup[matchingVersion];
            return result.Invoke();
        }

        private readonly IReadOnlyDictionary<Version, Func<T>> _lookup;
    }
}
