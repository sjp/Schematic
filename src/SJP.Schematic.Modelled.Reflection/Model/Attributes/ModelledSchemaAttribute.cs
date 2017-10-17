using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public abstract class ModelledSchemaAttribute : Attribute
    {
        protected ModelledSchemaAttribute(IEnumerable<Type> dialects)
        {
            if (dialects == null || dialects.Empty() || dialects.AnyNull())
                throw new ArgumentNullException(nameof(dialects));

            // make sure that we only pass in dialect types
            var incorrectTypes = dialects
                .Where(d => d != Dialect.All && !_dialectInterface.GetTypeInfo().IsAssignableFrom(d))
                .ToList();
            if (incorrectTypes.Count > 0)
            {
                var message = incorrectTypes.Count == 1
                    ? "A non-dialect type was provided. Type: "
                    : "Non dialect types were provided. Types: ";
                throw new ArgumentException(message + incorrectTypes.Select(t => t.FullName).Join(", "), nameof(dialects));
            }

            // if we encounter Dialect.All then must affect everything
            if (dialects.Any(d => d == Dialect.All))
            {
                dialects = Enumerable.Empty<Type>();
                AffectsAllDialects = true;
            }

            Dialects = dialects;
        }

        public IEnumerable<Type> Dialects { get; }

        public bool SupportsDialect(Type dialect)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (!_dialectInterface.GetTypeInfo().IsAssignableFrom(dialect))
                throw new ArgumentException($"The given type { dialect.FullName } is not a dialect type.", nameof(dialect));

            return AffectsAllDialects || Dialects.Any(d => d.GetTypeInfo().IsAssignableFrom(dialect));
        }

        private bool AffectsAllDialects { get; }

        private static readonly Type _dialectInterface = typeof(IDatabaseDialect);
    }
}
