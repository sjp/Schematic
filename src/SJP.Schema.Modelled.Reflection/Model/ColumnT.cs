using System;
using System.Reflection;
using SJP.Schema.Core;

namespace SJP.Schema.Modelled.Reflection.Model
{
    public class Column<T> : ModelledColumn
    {
        public Column()
            : base(GetDbTypeArg(typeof(T)), IsNullableTypeArg(typeof(T)))
        {
        }

        private static IDbType GetDbTypeArg(Type typeArg)
        {
            ValidateTypeArg(typeArg);

            var nullableType = Nullable.GetUnderlyingType(typeArg);
            var resolvedType = nullableType ?? typeArg;

            return (IDbType)Activator.CreateInstance(resolvedType);
        }

        private static bool IsNullableTypeArg(Type typeArg)
        {
            ValidateTypeArg(typeArg);
            return Nullable.GetUnderlyingType(typeArg) != null;
        }

        private static void ValidateTypeArg(Type typeArg)
        {
            if (!typeArg.GetTypeInfo().IsValueType)
                throw new ArgumentException("The type argument given to the column must be a value type. Type: " + typeArg.FullName);

            var nullableType = Nullable.GetUnderlyingType(typeArg);
            var resolvedType = nullableType ?? typeArg;

            if (!DbTypeArg.IsAssignableFrom(resolvedType))
                throw new ArgumentException($"The type argument given to the column must implement the { DbTypeArg.FullName } interface. Type: { typeArg.FullName }");
        }

        private static TypeInfo DbTypeArg { get; } = typeof(IDbType).GetTypeInfo();
    }
}
