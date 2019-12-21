using System;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.Poco.Tests
{
    [TestFixture]
    internal static class PocoDataAccessGeneratorTests
    {
        private const string TestCsprojFileName = "DataAccessGeneratorTest.csproj";

        [Test]
        public static void Ctor_GivenNullFileSystem_ThrowsArgumentNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new PocoDataAccessGenerator(null, database, commentProvider, nameTranslator));
        }

        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new PocoDataAccessGenerator(mockFs, null, commentProvider, nameTranslator));
        }

        [Test]
        public static void Ctor_GivenNullCommentProvider_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var nameTranslator = new VerbatimNameTranslator();
            Assert.Throws<ArgumentNullException>(() => new PocoDataAccessGenerator(mockFs, database, null, nameTranslator));
        }

        [Test]
        public static void Ctor_GivenNullNameTranslator_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            Assert.Throws<ArgumentNullException>(() => new PocoDataAccessGenerator(mockFs, database, commentProvider, null));
        }

        [Test]
        public static void Generate_GivenNullProjectPath_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new PocoDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);

            Assert.Throws<ArgumentNullException>(() => generator.Generate(null, "test"));
        }

        [Test]
        public static void Generate_GivenEmptyProjectPath_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new PocoDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);

            Assert.Throws<ArgumentNullException>(() => generator.Generate(string.Empty, "test"));
        }

        [Test]
        public static void Generate_GivenWhiteSpaceProjectPath_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new PocoDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);

            Assert.Throws<ArgumentNullException>(() => generator.Generate("    ", "test"));
        }

        [Test]
        public static void Generate_GivenNullNamespace_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new PocoDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFileName);

            Assert.Throws<ArgumentNullException>(() => generator.Generate(projectPath, null));
        }

        [Test]
        public static void Generate_GivenEmptyNamespace_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new PocoDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFileName);

            Assert.Throws<ArgumentNullException>(() => generator.Generate(projectPath, string.Empty));
        }

        [Test]
        public static void Generate_GivenWhiteSpaceNamespace_ThrowsArgumentNullException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new PocoDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFileName);

            Assert.Throws<ArgumentNullException>(() => generator.Generate(projectPath, "    "));
        }

        [Test]
        public static void Generate_GivenProjectPathNotACsproj_ThrowsArgumentException()
        {
            var mockFs = new MockFileSystem();
            var database = Mock.Of<IRelationalDatabase>();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new VerbatimNameTranslator();
            var generator = new PocoDataAccessGenerator(mockFs, database, commentProvider, nameTranslator);
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, "DataAccessGeneratorTest.vbproj");

            Assert.Throws<ArgumentException>(() => generator.Generate(projectPath, "test"));
        }
    }
}
