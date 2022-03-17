using NUnit.Framework;

namespace SJP.Schematic.Graphviz.Tests;

[TestFixture]
internal static class GraphvizTemporaryExecutableTests
{
    [Test]
    public static void DotPath_PropertyGet_IsNonEmptyOrWhiteSpace()
    {
        using var graphviz = new GraphvizTemporaryExecutable();
        var nonWhiteSpace = !string.IsNullOrWhiteSpace(graphviz.DotPath);

        Assert.That(nonWhiteSpace, Is.True);
    }

    [Test]
    public static void DotPath_PropertyGet_FileExists()
    {
        using var graphviz = new GraphvizTemporaryExecutable();
        Assert.That(graphviz.DotPath, Does.Exist);
    }

    [Test]
    public static void Dispose_WhenInvokedMoreThanOnce_DoesNotThrowError()
    {
        using var graphviz = new GraphvizTemporaryExecutable();
        Assert.That(() =>
        {
            graphviz.Dispose();
            graphviz.Dispose();
            graphviz.Dispose();
        }, Throws.Nothing);
    }
}
