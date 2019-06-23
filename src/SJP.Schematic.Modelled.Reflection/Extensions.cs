using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Modelled.Reflection.Model;

namespace SJP.Schematic.Modelled.Reflection
{
    /// <summary>
    /// Extension methods for using reflection objects in Schematic.
    /// </summary>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Obtain the backing field for an auto-implemented property.
        /// </summary>
        /// <param name="property">An auto-implemented property.</param>
        /// <param name="bindingFlags">Flags filtering the visibility of the property.</param>
        /// <returns>The field for an auto-implemented property if it exists.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="bindingFlags"/> is not a valid enumeration.</exception>
        public static FieldInfo GetAutoBackingField(this PropertyInfo property, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            if (!bindingFlags.IsValid())
                throw new ArgumentException($"The { nameof(BindingFlags) } provided must be a valid enum.", nameof(bindingFlags));

            var backingFieldName = $"<{ property.Name }>k__BackingField";
            var field = property.DeclaringType.GetField(backingFieldName, bindingFlags);

            var compilerAttr = field?.GetCustomAttribute<CompilerGeneratedAttribute>(true);
            return compilerAttr != null ? field : null;
        }

        /// <summary>
        /// Retrieves the default constructor method for a given type.
        /// </summary>
        /// <param name="type">The type which may contain a default constructor.</param>
        /// <returns>A <see cref="ConstructorInfo"/> object relating to the default constructor on <paramref name="type"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        public static ConstructorInfo GetDefaultConstructor(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.GetConstructor(Type.EmptyTypes);
        }

        /// <summary>
        /// Retrieves an attribute that applies to a specific dialect.
        /// </summary>
        /// <typeparam name="T">The attribute to retrieve.</typeparam>
        /// <param name="dialect">A dialect that the attribute should apply to.</param>
        /// <param name="property">The property that the attribute is applied to.</param>
        /// <returns>An attribute for the given property. This will be <c>null</c> when no attribute is present.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dialect"/> or <paramref name="property"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">More than one matching attribute was found.</exception>
        public static T GetDialectAttribute<T>(this IDatabaseDialect dialect, PropertyInfo property) where T : ModelledSchemaAttribute
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
                throw new ArgumentException($"More than one matching { typeof(T).FullName } attribute was found for the property { property.Name } in { property.ReflectedType.FullName }.", nameof(property));

            return attrs.SingleOrDefault();
        }

        /// <summary>
        /// Retrieves an attribute that applies to a specific dialect.
        /// </summary>
        /// <typeparam name="T">The attribute to retrieve.</typeparam>
        /// <param name="dialect">A dialect that the attribute should apply to.</param>
        /// <param name="type">The type of object that the attribute is applied to.</param>
        /// <returns>An attribute for the given object type. This will be <c>null</c> when no attribute is present.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dialect"/> or <paramref name="type"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">More than one matching attribute was found.</exception>
        public static T GetDialectAttribute<T>(this IDatabaseDialect dialect, Type type) where T : ModelledSchemaAttribute
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var dialectType = dialect.GetType();
            var attrs = type.GetCustomAttributes<T>(true)
                .Where(attr => attr.SupportsDialect(dialectType))
                .ToList();

            if (attrs.Count > 1)
                throw new ArgumentException($"More than one matching { typeof(T).FullName } attribute was found for { type.FullName }.", nameof(type));

            return attrs.SingleOrDefault();
        }

        /// <summary>
        /// Provides an alias for a property, or the property name.
        /// </summary>
        /// <param name="dialect">A dialect that the alias applies to.</param>
        /// <param name="property">A property that may contain an alias attribute.</param>
        /// <returns>A name that should be used for the property, which is an alias if one is available, or the property name otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dialect"/> or <paramref name="property"/> is <c>null</c>.</exception>
        public static string GetAliasOrDefault(this IDatabaseDialect dialect, PropertyInfo property)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            var aliasAttr = dialect.GetDialectAttribute<AliasAttribute>(property);
            return aliasAttr?.Alias ?? property.Name;
        }

        /// <summary>
        /// Provides an alias for a type, or the type name.
        /// </summary>
        /// <param name="dialect">A dialect that the alias applies to.</param>
        /// <param name="type">The type of object that the attribute is applied to.</param>
        /// <returns>An alias for a type if available, the type's name otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dialect"/> or <paramref name="type"/> is <c>null</c>.</exception>
        public static string GetAliasOrDefault(this IDatabaseDialect dialect, Type type)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var aliasAttr = dialect.GetDialectAttribute<AliasAttribute>(type);
            return aliasAttr?.Alias ?? type.Name;
        }

        /// <summary>
        /// Provides schema override for a type.
        /// </summary>
        /// <param name="dialect">A dialect that the schema override applies to.</param>
        /// <param name="type">The type of object that a schema override attribute may be applied to.</param>
        /// <returns>A schema override for a type if available, otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dialect"/> or <paramref name="type"/> is <c>null</c></exception>
        public static string GetSchemaOverride(this IDatabaseDialect dialect, Type type)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var nameAttr = dialect.GetDialectAttribute<SchemaAttribute>(type);
            return nameAttr?.Schema;
        }

        /// <summary>
        /// Retrieves the resolved schema-qualified name for an object type.
        /// </summary>
        /// <param name="dialect">A dialect that the name should be qualified for.</param>
        /// <param name="database">The database that an object should be qualified for.</param>
        /// <param name="type">The type of object that the attribute is applied to.</param>
        /// <returns>A schema-qualified name for a database object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="dialect"/>, <paramref name="database"/>, or <paramref name="type"/> is <c>null</c></exception>
        public static Identifier GetQualifiedNameOrDefault(this IDatabaseDialect dialect, IRelationalDatabase database, Type type)
        {
            if (dialect == null)
                throw new ArgumentNullException(nameof(dialect));
            if (database == null)
                throw new ArgumentNullException(nameof(dialect));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var schemaName = dialect.GetSchemaOverride(type);
            if (schemaName.IsNullOrWhiteSpace())
                schemaName = database.IdentifierDefaults.Schema;

            var localName = dialect.GetAliasOrDefault(type);
            return Identifier.CreateQualifiedIdentifier(schemaName, localName);
        }

        public static bool IsOptionType(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.IsGenericType && type.GetGenericTypeDefinition() == OptionDefinition;
        }

        private static readonly Type OptionDefinition = typeof(Option<>);
    }
}
