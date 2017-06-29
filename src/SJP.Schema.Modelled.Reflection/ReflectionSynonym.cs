using System;
using System.Linq;
using System.Reflection;
using SJP.Schema.Core;
using SJP.Schema.Modelled.Reflection.Model;

namespace SJP.Schema.Modelled.Reflection
{
    public class ReflectionSynonym : IDatabaseSynonym
    {
        public ReflectionSynonym(IRelationalDatabase database, Type synonymType)
        {
            if (database == null)
                throw new ArgumentException(nameof(database));

            SynonymType = synonymType ?? throw new ArgumentNullException(nameof(synonymType));
            var dialect = database.Dialect;
            Name = dialect.GetQualifiedNameOrDefault(database, SynonymType);

            var targetType = GetBaseGenericTypeArg();
            Target = dialect.GetQualifiedNameOrDefault(database, targetType);
        }

        public Identifier Name { get; }

        public Identifier Target { get; }

        protected Type SynonymType { get; }

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

        private static Type GenericSynonymType { get; } = typeof(Synonym<>);
    }
}
