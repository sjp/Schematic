using System;
using System.Linq;
using System.Reflection;
using SJP.Schema.Core;
using SJP.Schema.Modelled.Reflection.Model;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionSynonym<T> : IDatabaseSynonym
    {
        public ReflectionSynonym(IRelationalDatabase database)
        {
            if (database == null)
                throw new ArgumentException(nameof(database));

            var dialect = database.Dialect;
            Name = dialect.GetQualifiedNameOverrideOrDefault(database, SynonymType);

            var targetType = GetBaseGenericTypeArg();
            Target = dialect.GetQualifiedNameOverrideOrDefault(database, targetType);
        }

        public Identifier Name { get; }

        public Identifier Target { get; }

        private Type GetBaseGenericTypeArg()
        {
            var type = SynonymType;

            while (type.GetTypeInfo().BaseType != null)
            {
                type = type.GetTypeInfo().BaseType;
                if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == GenericSynonymType)
                    return type.GetTypeInfo().GetGenericArguments().Single();
            }

            throw new Exception($"Expected to find a synonym type that derived from Synonym<T>, but found that { SynonymType.FullName } does not derive from it.");
        }

        private static Type SynonymType { get; } = typeof(T);

        private static Type GenericSynonymType { get; } = typeof(Synonym<>);
    }
}
