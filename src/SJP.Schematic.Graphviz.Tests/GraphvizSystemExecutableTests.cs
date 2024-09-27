using System;
using NUnit.Framework;

namespace SJP.Schematic.Graphviz.Tests;

[TestFixture]
internal static class GraphvizSystemExecutableTests
{
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void Ctor_GivenNullOrWhiteSpace_ThrowsArgException(string path)
    {
        Assert.That(() => new GraphvizSystemExecutable(path), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void DotPath_PropertyGet_MatchesCtorArg()
    {
        const string exePath = "dot";
        using var graphviz = new GraphvizSystemExecutable(exePath);

        Assert.That(graphviz.DotPath, Is.EqualTo(exePath));
    }

    [Test]
    public static void Dispose_WhenInvokedMoreThanOnce_DoesNotThrowError()
    {
        using var graphviz = new GraphvizSystemExecutable("dot");
        Assert.That(() =>
        {
            graphviz.Dispose();
            graphviz.Dispose();
            graphviz.Dispose();
        }, Throws.Nothing);
    }
}