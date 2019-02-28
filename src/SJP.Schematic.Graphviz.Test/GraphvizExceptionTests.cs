using NUnit.Framework;

namespace SJP.Schematic.Graphviz.Tests
{
    public class GraphvizExceptionTests
    {
        [Test]
        public void ExitCode_PropertyGet_MatchesCtorArg()
        {
            const int expected = 123;
            var ex = new GraphvizException(expected, "Test");

            Assert.AreEqual(expected, ex.ExitCode);
        }

        [Test]
        public void Message_PropertyGet_MatchesCtorArg()
        {
            const string expected = "Test error message";
            var ex = new GraphvizException(123, expected);

            Assert.AreEqual(expected, ex.Message);
        }
    }
}