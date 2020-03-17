using NUnit.Framework;

namespace SJP.Schematic.Graphviz.Tests
{
    [TestFixture]
    internal static class GraphvizTemporaryExecutableTests
    {
        [Test]
        public static void DotExecutablePath_PropertyGet_IsNonEmptyOrWhiteSpace()
        {
            using var graphviz = new GraphvizTemporaryExecutable();
            var nonWhiteSpace = !string.IsNullOrWhiteSpace(graphviz.DotExecutablePath);

            Assert.That(nonWhiteSpace, Is.True);
        }

        [Test]
        public static void DotExecutablePath_PropertyGet_FileExists()
        {
            using var graphviz = new GraphvizTemporaryExecutable();
            Assert.That(graphviz.DotExecutablePath, Does.Exist);
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
}