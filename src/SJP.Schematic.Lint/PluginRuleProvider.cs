using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint
{
    /// <summary>
    /// A linting rule provider that scans its directory for other plugin providers. It then uses those to obtain rules for analysing database objects.
    /// </summary>
    /// <seealso cref="IRuleProvider" />
    public class PluginRuleProvider : IRuleProvider
    {
        /// <summary>
        /// Dynamically retrieves the rules used to analyze database objects. Searches the currently executing directory to obtain other rules.
        /// </summary>
        /// <param name="connection">A schematic connection.</param>
        /// <param name="level">The level used for reporting.</param>
        /// <returns>Rules used for analyzing database objects.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="connection"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="level"/> is not a valid enum value.</exception>
        public IEnumerable<IRule> GetRules(ISchematicConnection connection, RuleLevel level)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));
            if (!level.IsValid())
                throw new ArgumentException($"The { nameof(RuleLevel) } provided must be a valid enum.", nameof(level));

            var dialectType = connection.Dialect.GetType();

            return LoadPluginAssemblies()
                .SelectMany(a => LoadRequiredTypes(a, dialectType))
                .Select(t => Array.Find(t.GetConstructors(), c => c.IsPublic && c.GetParameters().Length == 0))
                .SelectMany(c => GetRules(c, connection, level))
                .ToList();
        }

        /// <summary>
        /// Filters down the retrieves set of rule provider types. Can be used for the purposes of overriding rules.
        /// </summary>
        /// <param name="type">The rule provider type.</param>
        /// <param name="dialectType">The type of the dialect.</param>
        /// <returns><c>true</c> if the rule provider type should be used; otherwise <c>false</c>.</returns>
        protected virtual bool TypeFilter(Type type, Type dialectType) => true;

        private static IEnumerable<Assembly> LoadPluginAssemblies()
        {
            var probingDir = Path.GetDirectoryName(AssemblyPath);
            return Directory.EnumerateFiles(probingDir, "*.dll", SearchOption.AllDirectories)
                .Where(a => a != AssemblyPath && IsLintPluginAssemblyPath(a))
                .Select(path =>
                {
                    try
                    {
#pragma warning disable S3885 // "Assembly.Load" should be used
                        return Assembly.LoadFrom(path);
#pragma warning restore S3885 // "Assembly.Load" should be used
                    }
                    catch
                    {
                        // can't load the assembly so return null to avoid failing on all assemblies
                        return null;
                    }
                })
                .Where(a => a != null)
                .Select(a => a!);
        }

        private IEnumerable<Type> LoadRequiredTypes(Assembly assembly, Type dialectType)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (dialectType == null)
                throw new ArgumentNullException(nameof(dialectType));

            try
            {
                return assembly.DefinedTypes
                    .Where(t => TypeFilter(t, dialectType)
                        && (IsRuleProviderImplementation(t) || IsDialectRuleProviderImplementation(t, dialectType)));
            }
            catch
            {
                // can't load/search types, so return empty set
                return Array.Empty<Type>();
            }
        }

        private static bool IsRuleProviderImplementation(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return (type.IsValueType || type.IsClass)
                && !type.IsAbstract
                && RuleProviderInterface.IsAssignableFrom(type);
        }

        private static bool IsDialectRuleProviderImplementation(Type type, Type dialectType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (dialectType == null)
                throw new ArgumentNullException(nameof(dialectType));

            return (type.IsValueType || type.IsClass)
                && !type.IsAbstract
                && type.GetTypeInfo().ImplementedInterfaces.Any(t => IsImplementedDialectRuleProviderInterface(t, dialectType));
        }

        private static bool IsImplementedDialectRuleProviderInterface(Type interfaceType, Type dialectType)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));
            if (dialectType == null)
                throw new ArgumentNullException(nameof(dialectType));

            return interfaceType.IsGenericType
                && interfaceType.GetGenericTypeDefinition() == DialectRuleProviderGeneric
                && interfaceType.GetGenericArguments().Length == 1
                && dialectType.IsAssignableFrom(interfaceType.GetGenericArguments().Single());
        }

        private static IEnumerable<IRule> GetRules(ConstructorInfo ruleProviderCtor, ISchematicConnection connection, RuleLevel level)
        {
            if (ruleProviderCtor == null)
                return Array.Empty<IRule>();

            var type = ruleProviderCtor.DeclaringType;
            var rulesMethod = type.GetMethod(nameof(IRuleProvider.GetRules), new[] { typeof(ISchematicConnection), typeof(RuleLevel) });
            var ruleProvider = ruleProviderCtor.Invoke(Array.Empty<object>());
            return rulesMethod != null
                ? (IEnumerable<IRule>)rulesMethod.Invoke(ruleProvider, new object[] { connection, level })
                : Array.Empty<IRule>();
        }

        /// <summary>
        /// Returns the core lint assembly's current location.
        /// </summary>
        protected static string AssemblyPath { get; } = Assembly.GetExecutingAssembly().Location;

        /// <summary>
        /// Determines whether a given file path is valid for searching for lint plugins. Primarily used to ignore system assemblies.
        /// </summary>
        /// <param name="assemblyFilePath">An assembly file path.</param>
        /// <returns><c>true</c> if the given file path is valid for searching for lint plugins; otherwise, <c>false</c>.</returns>
        protected static bool IsLintPluginAssemblyPath(string assemblyFilePath)
        {
            if (assemblyFilePath.IsNullOrWhiteSpace())
                return false;

            var fileName = Path.GetFileName(assemblyFilePath);
            return !IgnoredPrefixes.Any(prefix => fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        private static readonly IEnumerable<string> IgnoredPrefixes = new[] { "System.", "Microsoft." };

        private static readonly Type DialectRuleProviderGeneric = typeof(IDialectRuleProvider<>);

        private static readonly Type RuleProviderInterface = typeof(IRuleProvider);
    }
}
