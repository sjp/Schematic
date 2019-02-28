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

            Assert.IsTrue(nonWhiteSpace);
        }

        [Test]
        public void DotExecutablePath_PropertyGet_FileExists()
        {
            FileAssert.Exists(GraphvizExe.DotExecutablePath);
        }
    }
}