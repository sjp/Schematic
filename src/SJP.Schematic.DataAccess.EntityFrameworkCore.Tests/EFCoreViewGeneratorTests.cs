using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests
{
    [TestFixture]
    internal static class EFCoreViewGeneratorTests
    {
        private static IDatabaseViewGenerator GetViewGenerator() => new EFCoreViewGenerator(new MockFileSystem(), new VerbatimNameTranslator(), "SJP.Schematic.Test");

        [Test]
        public static void Ctor_GivenNullNameFileSystem_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();

            Assert.That(() => new EFCoreViewGenerator(null, nameTranslator, "test"), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            var fileSystem = new MockFileSystem();

            Assert.That(() => new EFCoreViewGenerator(fileSystem, null, "test"), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceNamespace_ThrowsArgumentNullException(string ns)
        {
            var fileSystem = new MockFileSystem();
            var nameTranslator = new VerbatimNameTranslator();

            Assert.That(() => new EFCoreViewGenerator(fileSystem, nameTranslator, ns), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNullDirectory_ThrowsArgumentNullException()
        {
            var generator = GetViewGenerator();

            Assert.That(() => generator.GetFilePath(null, "test"), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
        {
            var generator = GetViewGenerator();
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(tempDir.DirectoryPath));

            Assert.That(() => generator.GetFilePath(baseDir, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
        {
            var generator = GetViewGenerator();
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfoWrapper(new MockFileSystem(), new DirectoryInfo(tempDir.DirectoryPath));
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(tempDir.DirectoryPath, "Views", testViewName + ".cs");

            var filePath = generator.GetFilePath(baseDir, testViewName);

            Assert.That(filePath.FullName, Is.EqualTo(expectedPath));
        }

        [Test]
        public static void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
        {
            var generator = GetViewGenerator();
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfoWrapper(new MockFileSystem(), new DirectoryInfo(tempDir.DirectoryPath));
            const string testViewSchema = "view_schema";
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(tempDir.DirectoryPath, "Views", testViewSchema, testViewName + ".cs");

            var filePath = generator.GetFilePath(baseDir, new Identifier(testViewSchema, testViewName));

            Assert.That(filePath.FullName, Is.EqualTo(expectedPath));
        }

        [Test]
        public static void Generate_GivenNullView_ThrowsArgumentNullException()
        {
            var generator = GetViewGenerator();
            var comment = Option<IDatabaseViewComments>.None;

            Assert.That(() => generator.Generate(null, comment), Throws.ArgumentNullException);
        }
    }
}
