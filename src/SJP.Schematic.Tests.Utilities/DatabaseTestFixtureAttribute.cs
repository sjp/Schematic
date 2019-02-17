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

            if (!TypeCache.TryGetValue(target, out var propCache))
            {
                propCache = new ConcurrentDictionary<string, MethodInfo>();
                TypeCache.TryAdd(target, propCache);
            }

            if (!propCache.TryGetValue(propertyName, out var getMethod))
            {
                var propInfo = target.GetProperty(propertyName, SearchFlags);
                if (propInfo != null && (!IDbConnectionType.IsAssignableFrom(propInfo.PropertyType) || propInfo.GetGetMethod() == null))
                    propInfo = null;

                getMethod = propInfo?.GetGetMethod();
                propCache.TryAdd(propertyName, getMethod);
            }

            if (getMethod == null)
                return;

            if (!ResultCache.TryGetValue(getMethod, out var isEnabled))
            {
                isEnabled = (IDbConnection)getMethod.Invoke(null, Array.Empty<object>()) != null;
                ResultCache.TryAdd(getMethod, isEnabled);
            }

            if (!isEnabled)
                Ignore = ignoreMessage;
        }

        private const BindingFlags SearchFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Type IDbConnectionType = typeof(IDbConnection);
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, MethodInfo>> TypeCache = new ConcurrentDictionary<Type, ConcurrentDictionary<string, MethodInfo>>();
        private static readonly ConcurrentDictionary<MethodInfo, bool> ResultCache = new ConcurrentDictionary<MethodInfo, bool>();
    }
}
