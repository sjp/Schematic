using System;
using System.Collections.Generic;
using System.Linq;

namespace SJP.Schematic.DataAccess.Extensions
{
    /// <summary>
    /// Extension methods for working with a collection of strings.
    /// </summary>
    public static class EnumerableStringExtensions
    {
        /// <summary>
        /// Orders namespaces in a conventional manner. System namespaces appear first, followed by other namespaces (again, in order).
        /// </summary>
        /// <param name="namespaces">Namespaces to order.</param>
        /// <returns>An ordered set of namespaces.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="namespaces"/> is <c>null</c>.</exception>
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
