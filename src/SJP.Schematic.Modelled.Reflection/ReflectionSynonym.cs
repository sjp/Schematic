using System;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionSynonym : IDatabaseSynonym
    {
        public ReflectionSynonym(IRelationalDatabase database, Type synonymType)
        {
            if (database == null)
                throw new ArgumentException(nameof(database));
            if (synonymType == null)
                throw new ArgumentNullException(nameof(synonymType));

            var dialect = database.Dialect;
            Name = dialect.GetQualifiedNameOrDefault(database, synonymType);

            var targetType = GetBaseGenericTypeArg(synonymType);
            Target = dialect.GetQualifiedNameOrDefault(database, targetType);
        }

        public Identifier Name { get; }

        public Identifier Target { get; }

        private static Type GetBaseGenericTypeArg(Type synonymType)
        {
            var originalType = synonymType;
            var type = synonymType;

            while (type.BaseType != null)
            {
                type = type.BaseType;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == GenericSynonymType)
                    return type.GetGenericArguments().Single();
            }

            throw new Exception($"Expected to find a synonym type that derived from Synonym<T>, but found that { originalType.FullName } does not derive from it.");
        }

        private static Type GenericSynonymType { get; } = typeof(Synonym<>);
    }
}
