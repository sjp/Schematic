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
                var group = ns == "System" || ns.StartsWith("System.")
                    ? system
                    : nonSystem;
                group.Add(ns);
            }

            return system.OrderBy(n => n)
                .Concat(nonSystem.OrderBy(n => n))
                .ToList();
        }
    }
}
