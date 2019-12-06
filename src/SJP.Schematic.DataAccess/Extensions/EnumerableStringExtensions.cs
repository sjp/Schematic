using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.DataAccess.Extensions
{
    public static class EnumerableStringExtensions
    {
        public static IEnumerable<string> OrderNamespaces(this IEnumerable<string> namespaces)
        {
            if (namespaces == null)
                throw new ArgumentNullException(nameof(namespaces));

            var system = new List<string>();
            var nonSystem = new List<string>();

            foreach (var ns in namespaces)
            {
                var isSystem = ns == SystemNamespace || ns.StartsWith(SystemNamespacePrefix, StringComparison.Ordinal);
                var group = isSystem ? system : nonSystem;
                group.Add(ns);
            }

            return system.OrderBy(n => n)
                .Concat(nonSystem.OrderBy(n => n))
                .ToList();
        }

        private const string SystemNamespace = nameof(System);
        private const string SystemNamespacePrefix = nameof(System) + ".";
    }
}
