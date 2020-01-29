using System;
using LanguageExt;
using NUnit.Framework.Constraints;

namespace SJP.Schematic.Tests.Utilities
{

    public sealed class SomeConstraint : Constraint
    {
        public override string Description { get; protected set; } = "Some";

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            var actualType = typeof(TActual);
            if (!actualType.IsGenericType)
                throw new ArgumentException($"Expected an Option<T> object, instead received {actualType.FullName}", nameof(actual));

            var gen = actualType.GetGenericTypeDefinition();
            if (gen != OptionGeneric)
                throw new ArgumentException($"Expected an Option<T> object, instead received {actualType.FullName}", nameof(actual));

            var propGet = actualType.GetProperty(nameof(Option<object>.IsSome))!.GetGetMethod()!;
            var isSome = (bool)propGet.Invoke(actual, Array.Empty<object>())!;
            //return new ConstraintResult(this, actual, isSome);
            return new OptionConstraintResult(this, true, isSome);
        }

        private static readonly Type OptionGeneric = typeof(Option<>);
    }
}
