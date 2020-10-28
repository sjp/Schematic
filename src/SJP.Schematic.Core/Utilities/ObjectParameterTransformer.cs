using System;
using System.Collections.Generic;
using System.Reflection;
using Dapper;

namespace SJP.Schematic.Core.Utilities
{
    /// <summary>
    /// A utility class that contains methods used to make working with collections of query parameters easier.
    /// </summary>
    public static class ObjectParameterTransformer
    {
        /// <summary>
        /// Converts a parameter lookup to a set of parameters that can be used to query with Dapper.
        /// </summary>
        /// <param name="paramLookup">The parameter lookup.</param>
        /// <returns>A dynamic query parameters object for use with Dapper.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="paramLookup"/> is <c>null</c>.</exception>
        public static DynamicParameters ToParameters(IReadOnlyDictionary<string, object> paramLookup)
        {
            if (paramLookup == null)
                throw new ArgumentNullException(nameof(paramLookup));

            var result = new DynamicParameters();
            if (paramLookup.Count == 0)
                return result;

            foreach (var kv in paramLookup)
                result.Add(kv.Key, kv.Value);

            return result;
        }

        /// <summary>
        /// Converts any object to a dictionary lookup of strings (property names), to values.
        /// </summary>
        /// <param name="param">The parameter object.</param>
        /// <returns>A lookup of the object's constituent property values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="param"/> is <c>null</c>.</exception>
        /// <example>The following method call:
        /// <c>ObjectParameterTransformer.ToDictionary(new { A = "test", B = 132 })</c>
        /// will return an object equivalent to the following result:
        /// <c>new Dictionary&lt;string, object&gt;{ ["A"] = "test", ["B"] = 132 }</c>
        /// </example>
        public static IReadOnlyDictionary<string, object> ToDictionary(object param)
        {
            if (param == null)
                throw new ArgumentNullException(nameof(param));

            var paramType = param.GetType();
            var paramProps = paramType.GetProperties(PropFlags);

            var result = new Dictionary<string, object>(paramProps.Length, StringComparer.Ordinal);

            foreach (var prop in paramProps)
                result[prop.Name] = prop.GetValue(param);

            return result;
        }

        private const BindingFlags PropFlags = BindingFlags.FlattenHierarchy | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public;
    }
}
