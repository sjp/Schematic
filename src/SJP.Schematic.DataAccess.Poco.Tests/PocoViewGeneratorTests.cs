using System;
using System.IO;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Poco.Tests
{
    [TestFixture]
    internal class PocoViewGeneratorTests
    {
        [Test]
        public void Ctor_GivenNullNameProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PocoViewGenerator(null, "testns"));
        }

        [Test]
        public void Ctor_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new PocoViewGenerator(nameProvider, null));
        }

        [Test]
        public void Ctor_GivenEmptyNamespace_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new PocoViewGenerator(nameProvider, string.Empty));
        }

        [Test]
        public void Ctor_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new PocoViewGenerator(nameProvider, "   "));
        }

        [Test]
        public void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoViewGenerator(nameProvider, testNs);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);

            Assert.Throws<ArgumentNullException>(() => generator.GetFilePath(baseDir, null));
        }

        [Test]
        public void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
        {
            var nameProvider = new VerbatimNameProvider();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoViewGenerator(nameProvider, testNs);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Views", testViewName + ".cs");

            var filePath = generator.GetFilePath(baseDir, testViewName);

            Assert.AreEqual(expectedPath, filePath.FullName);
        }

        [Test]
        public void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
        {
            var nameProvider = new VerbatimNameProvider();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoViewGenerator(nameProvider, testNs);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testViewSchema = "view_schema";
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Views", testViewSchema, testViewName + ".cs");

            var filePath = generator.GetFilePath(baseDir, new Identifier(testViewSchema, testViewName));

            Assert.AreEqual(expectedPath, filePath.FullName);
        }

        [Test]
        public void Generate_GivenNullView_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoViewGenerator(nameProvider, testNs);

            Assert.Throws<ArgumentNullException>(() => generator.Generate(null));
        }
    }
}
