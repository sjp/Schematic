using NUnit.Framework;

namespace SJP.Schematic.Graphviz.Tests
{
    public class GraphvizTemporaryExecutableTests
    {
        private GraphvizTemporaryExecutable GraphvizExe { get; set; }

        [OneTimeSetUp]
        public void Init()
        {
            GraphvizExe = new GraphvizTemporaryExecutable();
        }

        [OneTimeTearDown]
        public void CleanUp()
        {
            GraphvizExe.Dispose();
        }

        [Test]
        public void DotExecutablePath_PropertyGet_IsNonEmptyOrWhiteSpace()
        {
            var nonWhiteSpace = !string.IsNullOrWhiteSpace(GraphvizExe.DotExecutablePath);

            Assert.That(nonWhiteSpace, Is.True);
        }

        [Test]
        public void DotExecutablePath_PropertyGet_FileExists()
        {
            Assert.That(GraphvizExe.DotExecutablePath, Does.Exist);
        }
    }
}