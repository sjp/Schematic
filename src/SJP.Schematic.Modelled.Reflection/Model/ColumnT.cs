using System;
using System.Reflection;

namespace SJP.Schematic.Modelled.Reflection.Model
{
    public class Column<T> : ModelledColumn
    {
        public Column()
            : base(GetDbTypeArg(typeof(T)), IsNullableTypeArg(typeof(T)))
        {
        }

        private static Type GetDbTypeArg(Type typeArg)
        {
            ValidateTypeArg(typeArg);

            var nullableType = Nullable.GetUnderlyingType(typeArg);
            return nullableType ?? typeArg;
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
        }
    }
}
