using System.IO;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.Tests
{
    [TestFixture]
    internal static class DatabaseViewGeneratorTests
    {
        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            Assert.That(() => new FakeDatabaseViewGenerator(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNullDirectory_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new FakeDatabaseViewGenerator(nameTranslator);

            Assert.That(() => generator.InnerGetFilePath(null, "test"), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new FakeDatabaseViewGenerator(nameTranslator);
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfo(tempDir.DirectoryPath);

            Assert.That(() => generator.InnerGetFilePath(baseDir, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new FakeDatabaseViewGenerator(nameTranslator);
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfo(tempDir.DirectoryPath);
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(tempDir.DirectoryPath, "Views", testViewName + ".cs");

            var filePath = generator.InnerGetFilePath(baseDir, testViewName);

            Assert.That(filePath.FullName, Is.EqualTo(expectedPath));
        }

        [Test]
        public static void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new FakeDatabaseViewGenerator(nameTranslator);
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfo(tempDir.DirectoryPath);
            const string testViewSchema = "view_schema";
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(tempDir.DirectoryPath, "Views", testViewSchema, testViewName + ".cs");

            var filePath = generator.InnerGetFilePath(baseDir, new Identifier(testViewSchema, testViewName));

            Assert.That(filePath.FullName, Is.EqualTo(expectedPath));
        }
    }
}
