using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SJP.Schematic.Core;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    public class ReflectionTypeProvider
    {
        public ReflectionTypeProvider(IDatabaseDialect dialect, Type databaseDefinitionType)
        {
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
            DatabaseDefinitionType = databaseDefinitionType ?? throw new ArgumentNullException(nameof(databaseDefinitionType));

            _tables = new Lazy<IEnumerable<Type>>(LoadTables);
            _views = new Lazy<IEnumerable<Type>>(LoadViews);
            _sequences = new Lazy<IEnumerable<Type>>(LoadSequences);
            _synonyms = new Lazy<IEnumerable<Type>>(LoadSynonyms);

            _dbProperties = DatabaseDefinitionType.GetProperties();
        }

        protected IDatabaseDialect Dialect { get; }

        protected Type DatabaseDefinitionType { get; }

        public IEnumerable<Type> Tables => _tables.Value;

        public IEnumerable<Type> Views => _views.Value;

        public IEnumerable<Type> Sequences => _sequences.Value;

        public IEnumerable<Type> Synonyms => _synonyms.Value;

        protected virtual IEnumerable<Type> LoadTables() => GetUnwrappedPropertyTypes(TableGenericType).ToList();

        protected virtual IEnumerable<Type> LoadViews() => GetUnwrappedPropertyTypes(ViewGenericType).ToList();

        protected virtual IEnumerable<Type> LoadSequences() => GetUnwrappedPropertyTypes(SequenceGenericType).ToList();

        protected virtual IEnumerable<Type> LoadSynonyms() => GetUnwrappedPropertyTypes(SynonymGenericType).ToList();

        protected static Type TableGenericType { get; } = typeof(Table<>);

        protected static Type ViewGenericType { get; } = typeof(View<>);

        protected static Type SequenceGenericType { get; } = typeof(Sequence<>);

        protected static Type SynonymGenericType { get; } = typeof(Synonym<>);

        protected IEnumerable<Type> GetUnwrappedPropertyTypes(Type objectType)
        {
            if (objectType == null)
                throw new ArgumentNullException(nameof(objectType));

            return _dbProperties
                .Where(pi => pi.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(objectType))
                .Select(static pi => UnwrapGenericParameter(pi.PropertyType))
                .Where(static t => !t.IsAbstract)
                .ToList();
        }

        protected static Type UnwrapGenericParameter(Type inputType) => inputType.GetGenericArguments().Single();

        private readonly Lazy<IEnumerable<Type>> _tables;
        private readonly Lazy<IEnumerable<Type>> _views;
        private readonly Lazy<IEnumerable<Type>> _sequences;
        private readonly Lazy<IEnumerable<Type>> _synonyms;

        private readonly IEnumerable<PropertyInfo> _dbProperties;
    }
}
