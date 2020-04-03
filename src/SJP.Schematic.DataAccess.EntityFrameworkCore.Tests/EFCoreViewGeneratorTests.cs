using System.IO;
using System.IO.Abstractions;
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
        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            Assert.That(() => new EFCoreViewGenerator(null, "test"), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Ctor_GivenNullOrWhiteSpaceNamespace_ThrowsArgumentNullException(string ns)
        {
            var nameTranslator = new VerbatimNameTranslator();
            Assert.That(() => new EFCoreViewGenerator(nameTranslator, ns), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNullDirectory_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string testNs = "SJP.Schematic.Test";
            var generator = new EFCoreViewGenerator(nameTranslator, testNs);

            Assert.That(() => generator.GetFilePath(null, "test"), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNullObjectName_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string testNs = "SJP.Schematic.Test";
            var generator = new EFCoreViewGenerator(nameTranslator, testNs);
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(tempDir.DirectoryPath));

            Assert.That(() => generator.GetFilePath(baseDir, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetFilePath_GivenNameWithOnlyLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string testNs = "SJP.Schematic.Test";
            var generator = new EFCoreViewGenerator(nameTranslator, testNs);
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(tempDir.DirectoryPath));
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(tempDir.DirectoryPath, "Views", testViewName + ".cs");

            var filePath = generator.GetFilePath(baseDir, testViewName);

            Assert.That(filePath.FullName, Is.EqualTo(expectedPath));
        }

        [Test]
        public static void GetFilePath_GivenNameWithSchemaAndLocalName_ReturnsExpectedPath()
        {
            var nameTranslator = new VerbatimNameTranslator();
            const string testNs = "SJP.Schematic.Test";
            var generator = new EFCoreViewGenerator(nameTranslator, testNs);
            using var tempDir = new TemporaryDirectory();
            var baseDir = new DirectoryInfoWrapper(new FileSystem(), new DirectoryInfo(tempDir.DirectoryPath));
            const string testViewSchema = "view_schema";
            const string testViewName = "view_name";
            var expectedPath = Path.Combine(tempDir.DirectoryPath, "Views", testViewSchema, testViewName + ".cs");

            var filePath = generator.GetFilePath(baseDir, new Identifier(testViewSchema, testViewName));

            Assert.That(filePath.FullName, Is.EqualTo(expectedPath));
        }

        [Test]
        public static void Generate_GivenNullView_ThrowsArgumentNullException()
        {
            var nameTranslator = new VerbatimNameTranslator();
            var comment = Option<IDatabaseViewComments>.None;
            const string testNs = "SJP.Schematic.Test";
            var generator = new EFCoreViewGenerator(nameTranslator, testNs);

            Assert.That(() => generator.Generate(null, comment), Throws.ArgumentNullException);
        }
    }
}
