using NUnit.Framework.Constraints;

namespace SJP.Schematic.Tests.Utilities;

/// <summary>
/// A constraint result which displays the value of an option type.
/// </summary>
/// <seealso cref="ConstraintResult" />
public sealed class OptionConstraintResult : ConstraintResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OptionConstraintResult"/> class.
    /// </summary>
    /// <param name="constraint">The constraint.</param>
    /// <param name="expectedSome">if set to <c>true</c>, a some value is expected, otherwise a none value is expected.</param>
    /// <param name="success">if set to <c>true</c> indicates whether the assertion was successful.</param>
    public OptionConstraintResult(IConstraint constraint, bool expectedSome, bool success)
        : base(constraint, success, success)
    {
        ExpectedSome = expectedSome;
    }

    private bool ExpectedSome { get; }

    /// <summary>
    /// Write the actual value for a failing constraint test to a MessageWriter. Unwraps and displays Some/None for valid types.
    /// </summary>
    /// <param name="writer">The writer on which the actual value is displayed</param>
    public override void WriteActualValueTo(MessageWriter writer)
    {
        var value = (bool)ActualValue;
        if (ExpectedSome)
        {
            writer.Write(value ? "Some" : "None");
        }
        else
        {
            writer.Write(value ? "None" : "Some");
        }
    }
}
