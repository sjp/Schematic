using System;
using System.Collections.Generic;
using System.Reflection;
using Dapper;

namespace SJP.Schematic.Core.Utilities
{
    public static class ObjectParameterTransformer
    {
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

        public static IReadOnlyDictionary<string, object> ToDictionary(object param)
        {
            if (param == null)
                throw new ArgumentNullException(nameof(param));

            var paramType = param.GetType();
            var paramProps = paramType.GetProperties(PropFlags);

            var result = new Dictionary<string, object>(paramProps.Length);

            foreach (var prop in paramProps)
                result[prop.Name] = prop.GetValue(param);

            return result;
        }

        private const BindingFlags PropFlags = BindingFlags.FlattenHierarchy | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public;
    }
}
