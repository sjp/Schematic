using System;
using System.IO;
using System.IO.Abstractions;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.OrmLite.Tests
{
    [TestFixture]
    internal static class OrmLiteTableGeneratorTests
    {
        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            Assert.That(() => new OrmLiteTableGenerator(null, "test"), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.That(() => new OrmLiteTableGenerator(nameTranslator, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.That(() => new OrmLiteTableGenerator(nameTranslator, string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.That(() => new OrmLiteTableGenerator(nameTranslator, "   "), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNullDirectory_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);

            Assert.That(() => generator.GetFilePath(null, "test"), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(tempDir.DirectoryPath));

            Assert.That(() => generator.GetFilePath(baseDir, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(tempDir.DirectoryPath));
            const string testTableName = "table_name";
            var expectedPath = Path.Combine(tempDir.DirectoryPath, "Tables", testTableName + ".cs");

            var filePath = generator.GetFilePath(baseDir, testTableName);

            Assert.That(filePath.FullName, Is.EqualTo(expectedPath));
        }

        [Test]
        public static void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(tempDir.DirectoryPath));
            const string testTableSchema = "table_schema";
            const string testTableName = "table_name";
            var expectedPath = Path.Combine(tempDir.DirectoryPath, "Tables", testTableSchema, testTableName + ".cs");

            var filePath = generator.GetFilePath(baseDir, new Identifier(testTableSchema, testTableName));

            Assert.That(filePath.FullName, Is.EqualTo(expectedPath));
        }

        [Test]
        public static void Generate_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);
            var table = Mock.Of<IRelationalDatabaseTable>();

            Assert.That(() => generator.Generate(null, table, Option<IRelationalDatabaseTableComments>.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void Generate_GivenNullTable_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string test = "SJP.Schematic.Test";
            var generator = new OrmLiteTableGenerator(nameTranslator, test);

            Assert.That(() => generator.Generate(Array.Empty<IRelationalDatabaseTable>(), null, Option<IRelationalDatabaseTableComments>.None), Throws.ArgumentNullException);
        }
    }
}
