using System;
using System.IO;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Poco.Tests
{
    [TestFixture]
    public class PocoTableGeneratorTests
    {
        [Test]
        public void Ctor_GivenNullNameProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PocoTableGenerator(null, "testns"));
        }

        [Test]
        public void Ctor_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new PocoTableGenerator(nameProvider, null));
        }

        [Test]
        public void Ctor_GivenEmptyNamespace_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new PocoTableGenerator(nameProvider, string.Empty));
        }

        [Test]
        public void Ctor_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            Assert.Throws<ArgumentNullException>(() => new PocoTableGenerator(nameProvider, "   "));
        }

        [Test]
        public void GetFilePath_GivenNullDirectory_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoTableGenerator(nameProvider, testNs);

            Assert.Throws<ArgumentNullException>(() => generator.GetFilePath(null, "test"));
        }

        [Test]
        public void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoTableGenerator(nameProvider, testNs);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);

            Assert.Throws<ArgumentNullException>(() => generator.GetFilePath(baseDir, null));
        }

        [Test]
        public void GetFilePath_GivenNullLocalName_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoTableGenerator(nameProvider, testNs);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);

            Assert.Throws<ArgumentNullException>(() => generator.GetFilePath(baseDir, new SchemaIdentifier("test")));
        }

        [Test]
        public void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
        {
            var nameProvider = new VerbatimNameProvider();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoTableGenerator(nameProvider, testNs);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testTableName = "table_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Tables", testTableName + ".cs");

            var filePath = generator.GetFilePath(baseDir, testTableName);

            Assert.AreEqual(expectedPath, filePath.FullName);
        }

        [Test]
        public void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
        {
            var nameProvider = new VerbatimNameProvider();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoTableGenerator(nameProvider, testNs);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testTableSchema = "table_schema";
            const string testTableName = "table_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Tables", testTableSchema, testTableName + ".cs");

            var filePath = generator.GetFilePath(baseDir, new Identifier(testTableSchema, testTableName));

            Assert.AreEqual(expectedPath, filePath.FullName);
        }

        [Test]
        public void Generate_GivenNullTable_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            const string testNs = "SJP.Schematic.Test";
            var generator = new PocoTableGenerator(nameProvider, testNs);

            Assert.Throws<ArgumentNullException>(() => generator.Generate(null));
        }
    }
}
