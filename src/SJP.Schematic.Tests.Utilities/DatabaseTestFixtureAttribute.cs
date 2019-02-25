using System;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using NUnit.Framework;

namespace SJP.Schematic.Tests.Utilities
{
    /// <summary>
    /// Annotates a type as being a test fixture conditional on a connection being available.
    /// </summary>
    [Category("SkipWhenLiveUnitTesting")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DatabaseTestFixtureAttribute : TestFixtureAttribute
    {
        /// <summary>
        /// Annotates a type as being a test fixture conditional on a connection being available.
        /// </summary>
        /// <param name="target">The type where an <see cref="IDbConnection"/> instance can be retrieved.</param>
        /// <param name="propertyName">The name of the property in <paramref name="target"/> where an <see cref="IDbConnection"/> instance can be retrieved.</param>
        /// <param name="ignoreMessage">The message to display when a test has been skipped due a missing connection.</param>
        public DatabaseTestFixtureAttribute(Type target, string propertyName, string ignoreMessage)
        {
            if (target == null || string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(ignoreMessage))
                return;

            var propCache = TypeCache.GetOrAdd(target, new ConcurrentDictionary<string, MethodInfo>());

            var getMethod = propCache.GetOrAdd(propertyName, propName =>
            {
                var propInfo = target.GetProperty(propName, SearchFlags);
                if (propInfo != null && (!IDbConnectionType.IsAssignableFrom(propInfo.PropertyType) || propInfo.GetGetMethod() == null))
                    propInfo = null;

                return propInfo?.GetGetMethod();
            });

            if (getMethod == null)
                return;

            var isEnabled = ResultCache.GetOrAdd(getMethod, method => (IDbConnection)method.Invoke(null, Array.Empty<object>()) != null);
            if (!isEnabled)
                Ignore = ignoreMessage;
        }

        private const BindingFlags SearchFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Type IDbConnectionType = typeof(IDbConnection);
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, MethodInfo>> TypeCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, MethodInfo>>();
        private static readonly ConcurrentDictionary<MethodInfo, bool> ResultCache = new ConcurrentDictionary<MethodInfo, bool>();
    }
}
