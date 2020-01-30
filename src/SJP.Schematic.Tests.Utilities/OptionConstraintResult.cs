using NUnit.Framework.Constraints;

namespace SJP.Schematic.Tests.Utilities
{
    public sealed class OptionConstraintResult : ConstraintResult
    {
        public OptionConstraintResult(IConstraint constraint, bool expectedSome, bool success)
            : base(constraint, success, success)
        {
            ExpectedSome = expectedSome;
        }

        private bool ExpectedSome { get; }

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
}
