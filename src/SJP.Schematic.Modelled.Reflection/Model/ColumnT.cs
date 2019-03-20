using System;

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
            if (nullableType != null)
                return nullableType;

            var isOptionType = typeArg.IsOptionType();
            return isOptionType
                ? typeArg.GetGenericArguments()[0]
                : typeArg;
        }

        private static bool IsNullableTypeArg(Type typeArg)
        {
            ValidateTypeArg(typeArg);
            return Nullable.GetUnderlyingType(typeArg) != null
                || typeArg.IsOptionType();
        }

        private static void ValidateTypeArg(Type typeArg)
        {
            if (!typeArg.IsGenericType)
                return;

            var isSimpleOption = typeArg.GetGenericArguments().Length == 1;
            if (!isSimpleOption)
                throw new ArgumentException("The type argument given to the column must be a db type, or an option of a db type. Type: " + typeArg.FullName);
        }
    }
}
