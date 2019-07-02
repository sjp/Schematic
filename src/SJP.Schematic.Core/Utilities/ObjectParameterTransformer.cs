using System;
using System.Collections.Generic;
using System.Reflection;

namespace SJP.Schematic.Core.Utilities
{
    public static class ObjectParameterTransformer
    {
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

        private const BindingFlags PropFlags = BindingFlags.FlattenHierarchy | System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
    }
}
