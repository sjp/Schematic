using System;
using LanguageExt;
using NUnit.Framework.Constraints;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// A constraint that asserts a <see cref="Option{A}.None"/> value must be present.
/// </summary>
/// <seealso cref="Constraint" />
public sealed class NoneConstraint : Constraint
{
    /// <summary>
    /// The Description of what this constraint tests, for
    /// use in messages and in the ConstraintResult. Always "None".
    /// </summary>
    public override string Description { get; protected set; } = "None";

    /// <summary>
    /// Applies the constraint to an actual value, returning a <see cref="ConstraintResult"/>.
    /// </summary>
    /// <param name="actual">The value to be tested</param>
    /// <returns>A <see cref="ConstraintResult"/></returns>
    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        var actualType = typeof(TActual);
        if (!actualType.IsGenericType)
            throw new ArgumentException($"Expected an Option<T> object, instead received {actualType.FullName}", nameof(actual));

        var gen = actualType.GetGenericTypeDefinition();
        if (gen != OptionGeneric)
            throw new ArgumentException($"Expected an Option<T> object, instead received {actualType.FullName}", nameof(actual));

        var propGet = actualType.GetProperty(nameof(Option<object>.IsNone))!.GetGetMethod()!;
        var isNone = (bool)propGet.Invoke(actual, Array.Empty<object>())!;

        return new OptionConstraintResult(this, false, isNone);
    }

    private static readonly Type OptionGeneric = typeof(Option<>);
}