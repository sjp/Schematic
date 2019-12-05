using System;
using System.IO;
using NUnit.Framework;
using SJP.Schematic.Core;

namespace SJP.Schematic.DataAccess.Tests
{
    [TestFixture]
    internal static class DatabaseTableGeneratorTests
    {
        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FakeDatabaseTableGenerator(null));
        }

        [Test]
        public static void GetFilePath_GivenNullDirectory_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new FakeDatabaseTableGenerator(nameTranslator);

            Assert.Throws<ArgumentNullException>(() => generator.InnerGetFilePath(null, "test"));
        }

        [Test]
        public static void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new FakeDatabaseTableGenerator(nameTranslator);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);

            Assert.Throws<ArgumentNullException>(() => generator.InnerGetFilePath(baseDir, null));
        }

        [Test]
        public static void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new FakeDatabaseTableGenerator(nameTranslator);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testTableName = "table_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Tables", testTableName + ".cs");

            var filePath = generator.InnerGetFilePath(baseDir, testTableName);

            Assert.AreEqual(expectedPath, filePath.FullName);
        }

        [Test]
        public static void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new FakeDatabaseTableGenerator(nameTranslator);
            var baseDir = new DirectoryInfo(Environment.CurrentDirectory);
            const string testTableSchema = "table_schema";
            const string testTableName = "table_name";
            var expectedPath = Path.Combine(Environment.CurrentDirectory, "Tables", testTableSchema, testTableName + ".cs");

            var filePath = generator.InnerGetFilePath(baseDir, new Identifier(testTableSchema, testTableName));

            Assert.AreEqual(expectedPath, filePath.FullName);
        }
    }
}
