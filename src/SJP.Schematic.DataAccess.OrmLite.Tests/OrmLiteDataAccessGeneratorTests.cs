using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.OrmLite.Tests
{
    [TestFixture]
    internal static class OrmLiteDataAccessGeneratorTests
    {
        private const string TestCsprojFileName = "DataAccessGeneratorTest.csproj";

        [Test]
        public static void Ctor_GivenNullFileSystem_ThrowsArgumentNullException()
        {
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var database = Mock.Of<IRelationalDatabase>();
            var nameTranslator = new VerbatimNameTranslator();

            Assert.That(() => new OrmLiteDataAccessGenerator(null, database, commentProvider, nameTranslator), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();

            Assert.That(() => new OrmLiteDataAccessGenerator(mockFs, null, commentProvider, nameTranslator), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullCommentProvider_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var nameTranslator = new VerbatimNameTranslator();

            Assert.That(() => new OrmLiteDataAccessGenerator(mockFs, database, null, nameTranslator), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();

            Assert.That(() => new OrmLiteDataAccessGenerator(mockFs, database, commentProvider, null), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Generate_GivenNullOrWhiteSpaceProjectPath_ThrowsArgumentNullException(string projectPath)
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new OrmLiteDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);

            Assert.That(() => generator.Generate(projectPath, "test"), Throws.ArgumentNullException);
        }

        [TestCase((string)null)]
        [TestCase("")]
        [TestCase("    ")]
        public static void Generate_GivenNullOrWhiteSpaceNamespace_ThrowsArgumentNullException(string ns)
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new OrmLiteDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFileName);

            Assert.That(() => generator.Generate(projectPath, ns), Throws.ArgumentNullException);
        }

        [Test]
        public static void Generate_GivenProjectPathNotACsproj_ThrowsArgumentException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new OrmLiteDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, "DataAccessGeneratorTest.vbproj");

            Assert.That(() => generator.Generate(projectPath, "test"), Throws.ArgumentException);
        }
    }
}
