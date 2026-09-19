using LanguageExt;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// A helper class for retrieving constraints for <see cref="Option{A}"/> values.
/// </summary>
public static class OptionIs
{
    /// <summary>
    /// A constraint which asserts that a result must be a none value.
    /// </summary>
    /// <value>The none asserting constraint.</value>
    public static NoneConstraint None => new();

    /// <summary>
    /// A constraint which asserts that a result must be a some value.
    /// </summary>
    /// <value>The some asserting constraint.</value>
    public static SomeConstraint Some => new();
}