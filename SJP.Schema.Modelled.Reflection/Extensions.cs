using System;
using System.Linq;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection
{
    public static class AutoSchemaReflectionExtensions
    {
        public static FieldInfo GetAutoBackingField(this PropertyInfo property, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var backingFieldName = $"<{ property.Name }>k__BackingField";
            return property.DeclaringType.GetTypeInfo().GetField(backingFieldName, bindingFlags);
        }

        public static T GetDialectAttribute<T>(this IDatabaseDialect dialect, PropertyInfo property) where T : AutoSchemaAttribute
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var dialectType = dialect.GetType();
            var attrs = property.GetCustomAttributes<T>(true)
                .Where(attr => attr.SupportsDialect(dialectType))
                .ToList();

            if (attrs.Count > 1)
                throw new ArgumentException($"More than one matching { typeof(T).FullName } attribute was found for the property { property.Name } in { property.DeclaringType.FullName }.", nameof(property));

            return attrs.SingleOrDefault();
        }

        public static T GetDialectAttribute<T>(this IDatabaseDialect dialect, Type type) where T : AutoSchemaAttribute
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var dialectType = dialect.GetType();
            var attrs = type.GetTypeInfo().GetCustomAttributes<T>(true)
                .Where(attr => attr.SupportsDialect(dialectType))
                .ToList();

            if (attrs.Count > 1)
                throw new ArgumentException($"More than one matching { typeof(T).FullName } attribute was found for { type.FullName }.", nameof(type));

            return attrs.SingleOrDefault();
        }

        public static string GetNameOverrideOrDefault(this IDatabaseDialect dialect, PropertyInfo property)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var nameAttr = dialect.GetDialectAttribute<NameAttribute>(property);
            return nameAttr?.Name ?? property.Name;
        }

        public static string GetNameOverrideOrDefault(this IDatabaseDialect dialect, Type type)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var nameAttr = dialect.GetDialectAttribute<NameAttribute>(type);
            return nameAttr?.Name ?? type.Name;
        }

        public static string GetSchemaOverride(this IDatabaseDialect dialect, Type type)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var nameAttr = dialect.GetDialectAttribute<SchemaAttribute>(type);
            return nameAttr?.Schema;
        }

        public static Identifier GetQualifiedNameOverrideOrDefault(this IDatabaseDialect dialect, IRelationalDatabase database, Type type)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (database == null)
                throw new ArgumentNullException(nameof(dialect));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var schemaName = dialect.GetSchemaOverride(type);
            if (schemaName.IsNullOrWhiteSpace())
                schemaName = database.DefaultSchema;

            var localName = dialect.GetNameOverrideOrDefault(type);
            return schemaName.IsNullOrWhiteSpace()
                ? new LocalIdentifier(localName)
                : new Identifier(schemaName, localName);
        }
    }
}
