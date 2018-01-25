using System;
using System.IO;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    [TestFixture]
    public class DatabaseTableGeneratorTests
    {
        [Test]
        public void Ctor_GivenNullNameProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeDatabaseTableGenerator(null));
        }

        [Test]
        public void GetFilePath_GivenNullDirectory_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            var generator = new FakeDatabaseTableGenerator(nameProvider);

            Assert.Throws<ArgumentNullException>(() => generator.InnerGetFilePath(null, "test"));
        }

        [Test]
        public void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            var generator = new FakeDatabaseTableGenerator(nameProvider);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);

            Assert.Throws<ArgumentNullException>(() => generator.InnerGetFilePath(baseDir, null));
        }

        [Test]
        public void GetFilePath_GivenNullLocalName_ThrowsArgumentNullException()
        {
            var nameProvider = new VerbatimNameProvider();
            var generator = new FakeDatabaseTableGenerator(nameProvider);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);

            Assert.Throws<ArgumentNullException>(() => generator.InnerGetFilePath(baseDir, new SchemaIdentifier("test")));
        }

        [Test]
        public void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
        {
            var nameProvider = new VerbatimNameProvider();
            var generator = new FakeDatabaseTableGenerator(nameProvider);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testTableName = "table_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Tables", testTableName + ".cs");

            var filePath = generator.InnerGetFilePath(baseDir, testTableName);

            Assert.AreEqual(expectedPath, filePath.FullName);
        }

        [Test]
        public void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
        {
            var nameProvider = new VerbatimNameProvider();
            var generator = new FakeDatabaseTableGenerator(nameProvider);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testTableSchema = "table_schema";
            const string testTableName = "table_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Tables", testTableSchema, testTableName + ".cs");

            var filePath = generator.InnerGetFilePath(baseDir, new Identifier(testTableSchema, testTableName));

            Assert.AreEqual(expectedPath, filePath.FullName);
        }
    }
}
