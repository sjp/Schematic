using NUnit.Framework;

namespace SJP.Schematic.Graphviz.Tests;

[TestFixture]
internal static class GraphvizExceptionTests
{
    [Test]
    public static void ExitCode_PropertyGet_MatchesCtorArg()
    {
        const int exitCode = 123;
        var ex = new GraphvizException(exitCode, "Test");

        Assert.That(ex.ExitCode, Is.EqualTo(exitCode));
    }

    [Test]
    public static void Message_PropertyGet_MatchesCtorArg()
    {
        const string message = "Test error message";
        var ex = new GraphvizException(123, message);

        Assert.That(ex.Message, Is.EqualTo(message));
    }
}